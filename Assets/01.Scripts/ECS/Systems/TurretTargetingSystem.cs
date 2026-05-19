using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyMovementSystem))]
public partial struct TurretTargetingSystem : ISystem
{
    private EntityQuery _enemyQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpatialIndexSingleton>();

        _enemyQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<EnemyTag>(),
            ComponentType.ReadOnly<LocalTransform>()
        );
    }

    public void OnUpdate(ref SystemState state)
    {
        var idx = SystemAPI.GetSingleton<SpatialIndexSingleton>();
        var healthLookup = SystemAPI.GetComponentLookup<Health>(false);
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                            .CreateCommandBuffer(state.WorldUnmanaged);

        state.CompleteDependency();

        state.Dependency = new TurretFireJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            ElapsedTime = (float)SystemAPI.Time.ElapsedTime,
            SpatialMap = idx.Map,
            SpatialOrigin = idx.Origin,
            SpatialCellSize = idx.CellSize,
            HealthLookup = healthLookup,
            ECB = ecb.AsParallelWriter()
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct TurretFireJob : IJobEntity
{
    public float DeltaTime;
    public float ElapsedTime;

    [ReadOnly] public NativeParallelMultiHashMap<int2, EnemySpatialEntry> SpatialMap;

    public float3 SpatialOrigin;
    public float SpatialCellSize;

    [NativeDisableParallelForRestriction]
    public ComponentLookup<Health> HealthLookup;

    public EntityCommandBuffer.ParallelWriter ECB;

    void Execute([ChunkIndexInQuery] int chunkIdx, in LocalTransform turretTransform, ref TurretStats stats)
    {
        stats.Cooldown -= DeltaTime;
        if (stats.Cooldown > 0f) return;

        int2 centerCell = SpatialIndexUtility.WorldToCell(
            turretTransform.Position, SpatialOrigin, SpatialCellSize);

        int cellRadius = (int)math.ceil(stats.Range / SpatialCellSize);

        float bestDistSq = stats.Range * stats.Range;
        Entity bestEnemy = Entity.Null;
        float3 bestPos = float3.zero;

        for (int dy = -cellRadius; dy <= cellRadius; dy++)
        {
            for (int dx = -cellRadius; dx <= cellRadius; dx++)
            {
                int2 cell = centerCell + new int2(dx, dy);

                if (!SpatialMap.TryGetFirstValue(cell, out var entry, out var it))
                    continue;

                do
                {
                    float3 d = entry.Position - turretTransform.Position;
                    d.y = 0f;

                    float distSq = math.lengthsq(d);

                    if (distSq < bestDistSq)
                    {
                        bestDistSq = distSq;
                        bestEnemy = entry.Entity;
                        bestPos = entry.Position;
                    }
                }
                while (SpatialMap.TryGetNextValue(out entry, ref it));
            }
        }

        if (bestEnemy == Entity.Null) return;

        float3 toEnemy = bestPos - turretTransform.Position;
        toEnemy.y = 0f;

        if (math.lengthsq(toEnemy) < 1e-6f) return;

        if (HealthLookup.HasComponent(bestEnemy))
        {
            Health hp = HealthLookup[bestEnemy];
            hp.Current -= stats.Damage;
            HealthLookup[bestEnemy] = hp;
        }

        var eventEntity = ECB.CreateEntity(chunkIdx);
        ECB.AddComponent(chunkIdx, eventEntity, new HitscanFiredEvent
        {
            Origin = turretTransform.Position,
            Hit = bestPos,
            MuzzleId = VfxId.GunnerMuzzle,
            BeamId = VfxId.GunnerBeam,
            HitSparkId = VfxId.GunnerHitSpark,
            SpawnTime = ElapsedTime,
        });

        stats.Cooldown = 1f / stats.FireRate;
    }
}