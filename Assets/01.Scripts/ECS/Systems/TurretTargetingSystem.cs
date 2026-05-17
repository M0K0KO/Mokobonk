using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyAISystem))]
public partial struct TurretTargetingSystem : ISystem
{
    private EntityQuery _enemyQuery;

    public void OnCreate(ref SystemState state)
    {
        _enemyQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<EnemyTag>(),
            ComponentType.ReadOnly<LocalTransform>()
        );
    }

    public void OnUpdate(ref SystemState state)
    {
        int enemyCount = _enemyQuery.CalculateEntityCount();
        if (enemyCount == 0) return;

        var enemyEntities = _enemyQuery.ToEntityArray(state.WorldUpdateAllocator);
        var enemyTransforms = _enemyQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);

        float dt = SystemAPI.Time.DeltaTime;
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        new TurretFireJob
        {
            DeltaTime = dt,
            EnemyEntities = enemyEntities,
            EnemyTransforms = enemyTransforms,
            ECB = ecb.AsParallelWriter()
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct TurretFireJob : IJobEntity
{
    public float DeltaTime;
    [ReadOnly] public NativeArray<Entity> EnemyEntities;
    [ReadOnly] public NativeArray<LocalTransform> EnemyTransforms;
    public EntityCommandBuffer.ParallelWriter ECB;

    void Execute([ChunkIndexInQuery] int chunkIdx, in LocalTransform turretTransform, ref TurretStats stats)
    {
        stats.Cooldown -= DeltaTime;
        if (stats.Cooldown > 0f) return;

        float bestDistSq = stats.Range * stats.Range;
        int bestIdx = -1;

        for (int i = 0; i < EnemyEntities.Length; i++)
        {
            float3 d = EnemyTransforms[i].Position - turretTransform.Position;
            d.y = 0f;
            float distSq = math.lengthsq(d);
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                bestIdx = i;
            }
        }

        if (bestIdx < 0) return;

        float3 toEnemy = EnemyTransforms[bestIdx].Position - turretTransform.Position;
        toEnemy.y = 0f;

        if (bestDistSq < 1e-6f) return;

        float3 dir = toEnemy * math.rsqrt(bestDistSq);

        var projectile = ECB.Instantiate(chunkIdx, stats.ProjectilePrefab);
        ECB.SetComponent(chunkIdx, projectile, LocalTransform.FromPosition(turretTransform.Position));
        ECB.SetComponent(chunkIdx, projectile, new ProjectileVelocity
        {
            Direction = dir,
            Speed = stats.ProjectileSpeed,
        });
        ECB.SetComponent(chunkIdx, projectile, new ProjectileDamage { Value = stats.Damage });

        stats.Cooldown = 1f / stats.FireRate;

    }
}