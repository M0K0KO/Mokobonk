using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[BurstCompile]
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
partial struct ProjectileHitSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        state.Dependency = new ProjectileHitJob
        {
            ProjectileTag = SystemAPI.GetComponentLookup<ProjectileTag>(true),
            ProjectileDamage = SystemAPI.GetComponentLookup<ProjectileDamage>(true),
            EnemyTag = SystemAPI.GetComponentLookup<EnemyTag>(true),
            HealthLookup = SystemAPI.GetComponentLookup<Health>(false),
            ECB = ecb.AsParallelWriter()
        }.Schedule(
            SystemAPI.GetSingleton<SimulationSingleton>(),
            state.Dependency
        );
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}

[BurstCompile]
public struct ProjectileHitJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<ProjectileTag> ProjectileTag;
    [ReadOnly] public ComponentLookup<ProjectileDamage> ProjectileDamage;
    [ReadOnly] public ComponentLookup<EnemyTag> EnemyTag;

    [NativeDisableParallelForRestriction] public ComponentLookup<Health> HealthLookup;
    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute(TriggerEvent triggerEvent)
    {
        var a = triggerEvent.EntityA;
        var b = triggerEvent.EntityB;

        if (ProjectileTag.HasComponent(a) && EnemyTag.HasComponent(b)) HandleHit(a, b);
        else if (ProjectileTag.HasComponent(b) && EnemyTag.HasComponent(a)) HandleHit(b, a);
    }

    private void HandleHit(Entity projectile, Entity enemy)
    {
        if (!HealthLookup.HasComponent(enemy)) return;

        var hp = HealthLookup[enemy];

        if (hp.Current < 0f) return;

        hp.Current -= ProjectileDamage[projectile].Value;
        HealthLookup[enemy] = hp;

        ECB.DestroyEntity(projectile.Index, projectile);
    }
}
