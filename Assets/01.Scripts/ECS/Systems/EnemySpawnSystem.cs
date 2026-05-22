using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
partial struct EnemySpawnSystem : ISystem
{
    private Random _rng;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemySpawnConfigSingleton>();
        state.RequireForUpdate<GridConfigSingleton>();
        state.RequireForUpdate<EnemyRegistrySingleton>();
        _rng = new Random(0x9E3779B9u);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var bal = SystemAPI.GetSingleton<BalanceMultiplierSingleton>();
        var spawnRW = SystemAPI.GetSingletonRW<EnemySpawnConfigSingleton>();
        ref var cfg = ref spawnRW.ValueRW;
        if (cfg.RemainingToSpawn <= 0) return;

        float now = (float)SystemAPI.Time.ElapsedTime;
        if (now < cfg.NextSpawnTime) return;

        cfg.NextSpawnTime = now + cfg.SpawnInterval / bal.SpawnRateMul;
        cfg.RemainingToSpawn--;

        var grid = SystemAPI.GetSingleton<GridConfigSingleton>();
        var registry = SystemAPI.GetSingleton<EnemyRegistrySingleton>();

        Entity prefab = PickRandomEnemyPrefab(ref _rng, registry, bal.WalkerRatio);

        int side = _rng.NextInt(0, 4);
        int2 cell = PickCellOnSide(side, grid.GridSize, ref _rng);
        float3 spawnPos = GridUtility.CellToWorld(cell, grid.Origin, grid.CellSize);

        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                            .CreateCommandBuffer(state.WorldUnmanaged);

        var enemy = ecb.Instantiate(prefab);
        var prefabTransform = SystemAPI.GetComponent<LocalTransform>(prefab);
        prefabTransform.Position = spawnPos;
        ecb.SetComponent(enemy, prefabTransform);
        var prefabHealth = SystemAPI.GetComponent<Health>(prefab);
        ecb.SetComponent(enemy, new Health
        {
            Current = prefabHealth.Max * bal.EnemyHpMul,
            Max = prefabHealth.Max * bal.EnemyHpMul,
        });

        var vatAsset = SystemAPI.GetComponent<VATAsset>(prefab);
        float walkDuration = vatAsset.Blob.Value.Clips[0].Duration;
        float randomOffset = _rng.NextFloat(0f, walkDuration);
        ecb.SetComponent(enemy, new VATAnimationState
        {
            CurrentClipIndex = 0,
            CurrentClipStartTime = -randomOffset
        });
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }

    private static Entity PickRandomEnemyPrefab(ref Random rng, in EnemyRegistrySingleton registry, float walkerRatio)
    {
        float roll = rng.NextFloat(0f, 1f);

        if (roll < walkerRatio)
        {
            if (registry.Map.TryGetValue((byte)EnemyKind.Walker, out var w)) return w.Prefab;
        }

        if (registry.Map.TryGetValue((byte)EnemyKind.Runner, out var r)) return r.Prefab;
        return Entity.Null;
    }

    private static int2 PickCellOnSide(int side, int2 size, ref Random rng)
    {
        return side switch
        {
            0 => new int2(rng.NextInt(0, size.x), size.y - 1),  // top (max Z)
            1 => new int2(size.x - 1, rng.NextInt(0, size.y)),  // right (max X)
            2 => new int2(rng.NextInt(0, size.x), 0),           // bottom (Z=0)
            _ => new int2(0, rng.NextInt(0, size.y)),           // left (X=0)
        };
    }
}
