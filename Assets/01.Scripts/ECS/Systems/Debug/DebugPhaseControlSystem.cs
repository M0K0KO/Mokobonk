using Unity.Burst;
using Unity.Entities;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(WaveSystem))]
partial struct DebugPhaseControlSystem : ISystem
{
    private EntityQuery _enemyQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WaveStateSingleton>();
        state.RequireForUpdate<WaveConfigSingleton>();
        state.RequireForUpdate<EnemySpawnConfigSingleton>();

        _enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyTag>().Build();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var waveRW = SystemAPI.GetSingletonRW<WaveStateSingleton>();
        ref var wave = ref waveRW.ValueRW;
        var spawnRW = SystemAPI.GetSingletonRW<EnemySpawnConfigSingleton>();
        var cfg = SystemAPI.GetSingleton<WaveConfigSingleton>();
        float now = (float)SystemAPI.Time.ElapsedTime;

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (force, entity) in SystemAPI.Query<RefRO<ForcePhaseTransition>>().WithEntityAccess())
        {
            switch (force.ValueRO.Target)
            {
                case GamePhase.Horde when wave.Phase == WavePhase.Preparing:
                    wave.NextWaveTime = now;
                    break;
                case GamePhase.Preparation when wave.Phase != WavePhase.Preparing:
                    ecb.DestroyEntity(_enemyQuery, EntityQueryCaptureMode.AtPlayback);
                    spawnRW.ValueRW.RemainingToSpawn = 0;
                    wave.RemainingEnemies = 0;
                    wave.Phase = WavePhase.Preparing;
                    wave.NextWaveTime = now + cfg.PrepareTime;
                    break;
            }
            ecb.DestroyEntity(entity);
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
}
#endif