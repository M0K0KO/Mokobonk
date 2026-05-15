using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
partial struct EnemySpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameConfig>();
        state.RequireForUpdate<PlayerPositionSingleton>();

        var spawnState = new SpawnState
        {
            Timer = 0f,
            random = Unity.Mathematics.Random.CreateFromIndex(1234),
            currentEnemyCount = 0
        };
        state.EntityManager.CreateSingleton(spawnState);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<GameConfig>();
        var playerPos = SystemAPI.GetSingleton<PlayerPositionSingleton>().Position;
        var spawnStateRW = SystemAPI.GetSingletonRW<SpawnState>();

        spawnStateRW.ValueRW.Timer += SystemAPI.Time.DeltaTime;
        if (spawnStateRW.ValueRW.Timer < config.spawnInterval) return;
        if (spawnStateRW.ValueRW.currentEnemyCount >= config.maxEnemies) return;

        spawnStateRW.ValueRW.Timer = 0f;

        var angle = spawnStateRW.ValueRW.random.NextFloat(0f, math.PI * 2f);
        var radius = spawnStateRW.ValueRW.random.NextFloat(config.spawnRadiusMin, config.spawnRadiusMax);
        var offset = new float3(math.cos(angle) * radius, 0f, math.sin(angle) * radius);
        var spawnPos = playerPos + offset;

        var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        var newEnemy = ecb.Instantiate(config.enemyPrefab);
        ecb.SetComponent(newEnemy, LocalTransform.FromPosition(spawnPos));

        spawnStateRW.ValueRW.currentEnemyCount++;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
