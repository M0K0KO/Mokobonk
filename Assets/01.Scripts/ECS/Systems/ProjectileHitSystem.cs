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
    private ComponentLookup<ProjectileTag> projectileLookup;
    private ComponentLookup<EnemyTag> enemyLookup;
    private ComponentLookup<Health> healthLookup;
    private ComponentLookup<ProjectileDamage> damageLookup;
    private ComponentLookup<Lifetime> lifetimeLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        projectileLookup = state.GetComponentLookup<ProjectileTag>(true);
        enemyLookup = state.GetComponentLookup<EnemyTag>(true);
        healthLookup = state.GetComponentLookup<Health>(false);       // write
        damageLookup = state.GetComponentLookup<ProjectileDamage>(true);
        lifetimeLookup = state.GetComponentLookup<Lifetime>(false);   // write

        state.RequireForUpdate<SimulationSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        projectileLookup.Update(ref state);
        enemyLookup.Update(ref state);
        healthLookup.Update(ref state);
        damageLookup.Update(ref state);
        lifetimeLookup.Update(ref state);

        var simulation = SystemAPI.GetSingleton<SimulationSingleton>();

        state.Dependency = new ProjectileHitJob
        {
            projectileLookup = projectileLookup,
            enemyLookup = enemyLookup,
            healthLookup = healthLookup,
            damageLookup = damageLookup,
            lifetimeLookup = lifetimeLookup,
        }.Schedule(simulation, state.Dependency);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}


[BurstCompile]
public struct ProjectileHitJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<ProjectileTag> projectileLookup;
    [ReadOnly] public ComponentLookup<EnemyTag> enemyLookup;
    public ComponentLookup<Health> healthLookup;
    [ReadOnly] public ComponentLookup<ProjectileDamage> damageLookup;
    public ComponentLookup<Lifetime> lifetimeLookup;
    public void Execute(TriggerEvent triggerEvent)
    {
        var a = triggerEvent.EntityA;
        var b = triggerEvent.EntityB;

        Entity projectile, enemy;
        if (projectileLookup.HasComponent(a) && enemyLookup.HasComponent(b))
        {
            projectile = a; enemy = b;
        }
        else if (projectileLookup.HasComponent(b) && enemyLookup.HasComponent(a))
        {
            projectile = b; enemy = a;
        }
        else return;

        var damage = damageLookup[projectile].Value;
        var health = healthLookup[enemy];
        health.Current -= damage;
        healthLookup[enemy] = health;

        var lifetime = lifetimeLookup[projectile];
        lifetime.Remaining = 0f;
        lifetimeLookup[projectile] = lifetime;
    }
}
