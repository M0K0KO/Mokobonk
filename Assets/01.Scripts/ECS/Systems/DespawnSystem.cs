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

        int killedThisFrame = 0;
        foreach (var (health, entity) in SystemAPI.Query<RefRO<Health>>().WithAll<EnemyTag>().WithEntityAccess())
        {
            if (health.ValueRO.Current <= 0f)
            {
                ecb.DestroyEntity(entity);
                killedThisFrame++;
            }
        }

        if (killedThisFrame >0)
        {
            if (SystemAPI.HasSingleton<WaveStateSingleton>())
            {
                var waveRW = SystemAPI.GetSingletonRW<WaveStateSingleton>();
                waveRW.ValueRW.AliveEnemies -= killedThisFrame;
            }

            if (SystemAPI.HasSingleton<ResourceSingleton>())
            {
                var resRW = SystemAPI.GetSingletonRW<ResourceSingleton>();
                resRW.ValueRW.Gold += resRW.ValueRO.EnemyKillReward * killedThisFrame;
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
