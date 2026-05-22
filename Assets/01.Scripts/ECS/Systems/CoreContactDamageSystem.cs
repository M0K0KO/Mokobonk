using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyMovementSystem))]
partial struct CoreContactDamageSystem : ISystem
{
    private const float CoreContactRadius = 1.25f;
    private const float CoreContactRadiusSq = CoreContactRadius * CoreContactRadius;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CoreDamageQueueSingleton>();
        state.RequireForUpdate<CorePositionSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var queue = SystemAPI.GetSingleton<CoreDamageQueueSingleton>().Queue;
        float3 corePos = SystemAPI.GetSingleton<CorePositionSingleton>().Value;

        var handle = new CoreContactDamageJob
        {
            CorePos = corePos,
            CoreContactRadiusSq = CoreContactRadiusSq,
            DamageQueue = queue.AsParallelWriter()
        }.ScheduleParallel(state.Dependency);

        handle.Complete();

        if (queue.Count == 0) return;

        float totalDamage = 0f;
        while (queue.TryDequeue(out float dmg))
            totalDamage += dmg;

        foreach (var coreHealth in SystemAPI.Query<RefRW<Health>>().WithAll<CoreTag>())
        {
            coreHealth.ValueRW.Current -= totalDamage;
            if (coreHealth.ValueRO.Current <= 0f)
                SystemAPI.GetSingletonRW<GameStateSingleton>().ValueRW.State = GameState.Lost;
        }
    }
        
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}


[BurstCompile]
[WithAll(typeof(EnemyTag))]
[WithNone(typeof(DyingTag))]
public partial struct CoreContactDamageJob : IJobEntity
{
    [ReadOnly] public float3 CorePos;
    [ReadOnly] public float CoreContactRadiusSq;
    public NativeQueue<float>.ParallelWriter DamageQueue;

    private void Execute(in LocalTransform transform, in ContactDamage contactDamage, ref Health health)
    {
        float3 delta = transform.Position - CorePos;
        delta.y = 0f;
        if (math.lengthsq(delta) > CoreContactRadiusSq) return;

        DamageQueue.Enqueue(contactDamage.Value);
        health.Current = -1f;
    }
}
