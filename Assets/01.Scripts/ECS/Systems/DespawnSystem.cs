using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
partial struct DespawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        float dt = SystemAPI.Time.DeltaTime;

        foreach (var (lt, entity) in SystemAPI.Query<RefRW<Lifetime>>().WithEntityAccess())
        {
            lt.ValueRW.Remaining -= dt;
            if (lt.ValueRO.Remaining <= 0f)
                ecb.DestroyEntity(entity);
        }


        foreach (var (health, entity) in SystemAPI.Query<RefRO<Health>>().WithAll<EnemyTag>().WithEntityAccess())
        {
            if (health.ValueRO.Current <= 0f)
                ecb.DestroyEntity(entity);
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
