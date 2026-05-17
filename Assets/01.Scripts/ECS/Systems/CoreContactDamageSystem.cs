using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[BurstCompile]
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
partial struct CoreContactDamageSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
        state.RequireForUpdate<CoreDamageQueueSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                                   .CreateCommandBuffer(state.WorldUnmanaged);

        var coreTagLookup = SystemAPI.GetComponentLookup<CoreTag>(true);
        var enemyTagLookup = SystemAPI.GetComponentLookup<EnemyTag>(true);
        var healthLookup = SystemAPI.GetComponentLookup<Health>(false);
        var contactDamageLookup = SystemAPI.GetComponentLookup<ContactDamage>(true);

        var queue = SystemAPI.GetSingleton<CoreDamageQueueSingleton>().Queue;

        var jobHandle = new CoreContactJob
        {
            CoreTag = coreTagLookup,
            EnemyTag = enemyTagLookup,
            Health = healthLookup,
            ContactDamage = contactDamageLookup,
            DamageQueue = queue.AsParallelWriter(),
            ECB = ecb.AsParallelWriter()
        }.Schedule(
                   SystemAPI.GetSingleton<SimulationSingleton>(),
                   state.Dependency
               );

        jobHandle.Complete();

        if (queue.Count == 0) { state.Dependency = jobHandle; return; }

        float totalDamage = 0f;
        while (queue.TryDequeue(out float dmg))
            totalDamage += dmg;

        foreach (var health in SystemAPI.Query<RefRW<Health>>().WithAll<CoreTag>())
        {
            health.ValueRW.Current -= totalDamage;
            if (health.ValueRO.Current <= 0f)
                SystemAPI.GetSingletonRW<GameStateSingleton>().ValueRW.State = GameState.Lost;
        }

        state.Dependency = jobHandle;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}


[BurstCompile]
public struct CoreContactJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<CoreTag> CoreTag;
    [ReadOnly] public ComponentLookup<EnemyTag> EnemyTag;
    public ComponentLookup<Health> Health;
    [ReadOnly] public ComponentLookup<ContactDamage> ContactDamage;
    public NativeQueue<float>.ParallelWriter DamageQueue;
    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute(TriggerEvent triggerEvent)
    {
        var a = triggerEvent.EntityA;
        var b = triggerEvent.EntityB;

        if (CoreTag.HasComponent(a) && EnemyTag.HasComponent(b)) HandleHit(a, b);
        else if (CoreTag.HasComponent(b) && EnemyTag.HasComponent(a)) HandleHit(b, a);
    }

    private void HandleHit(Entity core, Entity enemy)
    {
        if (!ContactDamage.HasComponent(enemy)) return;

        float dmg = ContactDamage[enemy].Value;
        DamageQueue.Enqueue(dmg);

        Health[enemy] = new Health { Current = -1f };
    }
}
