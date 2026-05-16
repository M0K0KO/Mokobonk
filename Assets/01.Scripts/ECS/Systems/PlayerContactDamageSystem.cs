using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[BurstCompile]
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
partial struct PlayerContactDamageSystem : ISystem
{
    private ComponentLookup<PlayerTag> playerLookup;
    private ComponentLookup<EnemyTag> enemyLookup;
    private ComponentLookup<ContactDamage> damageLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        playerLookup = state.GetComponentLookup<PlayerTag>(true);
        enemyLookup = state.GetComponentLookup<EnemyTag>(true);
        damageLookup = state.GetComponentLookup<ContactDamage>(true);

        state.RequireForUpdate<SimulationSingleton>();
        state.RequireForUpdate<PlayerDamageQueueSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        playerLookup.Update(ref state);
        enemyLookup.Update(ref state);
        damageLookup.Update(ref state);

        var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
        var queue = SystemAPI.GetSingleton<PlayerDamageQueueSingleton>().queue;

        state.Dependency = new PlayerContactDamageJob
        {
            playerLookup = playerLookup,
            enemyLookup = enemyLookup,
            damageLookup = damageLookup,
            damageQueue = queue.AsParallelWriter(),
        }.Schedule(simulation, state.Dependency);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}


[BurstCompile]
public struct PlayerContactDamageJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<PlayerTag> playerLookup;
    [ReadOnly] public ComponentLookup<EnemyTag> enemyLookup;
    [ReadOnly] public ComponentLookup<ContactDamage> damageLookup;
    public NativeQueue<float>.ParallelWriter damageQueue;

    public void Execute(TriggerEvent triggerEvent)
    {
        var a = triggerEvent.EntityA;
        var b = triggerEvent.EntityB;

        Entity enemy;
        if (playerLookup.HasComponent(a) && enemyLookup.HasComponent(b))
        {
            enemy = b;
        }
        else if (playerLookup.HasComponent(b) && enemyLookup.HasComponent(a))
        {
            enemy = a;
        }
        else return;

        var dmg = damageLookup[enemy].Value;
        damageQueue.Enqueue(dmg);
    }
}
