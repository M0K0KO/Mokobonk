using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(EnemySpawnSystem))]
partial struct WaveSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WaveStateSingleton>();
        state.RequireForUpdate<WaveConfigSingleton>();
        state.RequireForUpdate<EnemySpawnConfigSingleton>();
        state.RequireForUpdate<GameStateSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gameState = SystemAPI.GetSingleton<GameStateSingleton>();
        if (gameState.State != GameState.Playing) return;

        var waveRW = SystemAPI.GetSingletonRW<WaveStateSingleton>();
        ref var wave = ref waveRW.ValueRW;
        var cfg = SystemAPI.GetSingleton<WaveConfigSingleton>();
        var spawnRW = SystemAPI.GetSingletonRW<EnemySpawnConfigSingleton>();

        float now = (float)SystemAPI.Time.ElapsedTime;

        switch (wave.Phase)
        {
            case WavePhase.Preparing:
                if (wave.NextWaveTime <= 0f)
                    wave.NextWaveTime = now + cfg.PrepareTime;

                if (now >= wave.NextWaveTime)
                {
                    wave.CurrentWave++;
                    int count = cfg.BaseEnemyCount + (wave.CurrentWave - 1) * cfg.EnemyCountPerWave;
                    float interval = math.max(cfg.MinSpawnInterval, cfg.BaseSpawnInterval - (wave.CurrentWave - 1) * cfg.IntervalDecayPerWave);

                    spawnRW.ValueRW.RemainingToSpawn = count;
                    spawnRW.ValueRW.SpawnInterval = interval;
                    spawnRW.ValueRW.NextSpawnTime = now;
                    wave.AliveEnemies = count;
                    wave.Phase = WavePhase.Spawning;
                }
                break;

            case WavePhase.Spawning:
                if (spawnRW.ValueRO.RemainingToSpawn <= 0f)
                    wave.Phase = WavePhase.Clearing;
                break;

            case WavePhase.Clearing:
                if (wave.AliveEnemies <= 0)
                {
                    wave.Phase = WavePhase.Preparing;
                    wave.NextWaveTime = now + cfg.PrepareTime;
                }
                break;
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
