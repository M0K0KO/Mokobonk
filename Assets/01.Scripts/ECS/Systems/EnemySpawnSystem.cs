using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
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
        _rng = new Random(0x9E3779B9u);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var spawnRW = SystemAPI.GetSingletonRW<EnemySpawnConfigSingleton>();
        ref var cfg = ref spawnRW.ValueRW;

        float now = (float)SystemAPI.Time.ElapsedTime;
        if (now < cfg.NextSpawnTime) return;
        cfg.NextSpawnTime = now + cfg.SpawnInterval;

        var grid = SystemAPI.GetSingleton<GridConfigSingleton>();

        int side = _rng.NextInt(0, 4);
        int2 cell = PickCellOnSide(side, grid.GridSize, ref _rng);
        float3 spawnPos = GridUtility.CellToWorld(cell, grid.Origin, grid.CellSize);

        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                            .CreateCommandBuffer(state.WorldUnmanaged);

        var enemy = ecb.Instantiate(cfg.EnemyPrefab);
        ecb.SetComponent(enemy, LocalTransform.FromPosition(spawnPos));
        ecb.AddSharedComponent(enemy, new PhysicsWorldIndex { Value = 0 });
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
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
