using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyMovementSystem))]
public partial struct MortarTargetingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpatialIndexSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var idx = SystemAPI.GetSingleton<SpatialIndexSingleton>();
        var bal = SystemAPI.GetSingleton<BalanceMultiplierSingleton>();
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                            .CreateCommandBuffer(state.WorldUnmanaged);

        float elapsedTime = (float)SystemAPI.Time.ElapsedTime;

        state.CompleteDependency();

        state.Dependency = new MortarFireJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            ElapsedTime = elapsedTime,
            SpatialMap = idx.Map,
            SpatialOrigin = idx.Origin,
            SpatialCellSize = idx.CellSize,
            ECB = ecb.AsParallelWriter(),
            DamageMul = bal.TurretDamageMul,
            FireRateMul = bal.TurretFireRateMul,
            RangeMul = bal.TurretRangeMul,
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct MortarFireJob : IJobEntity
{
    public float DeltaTime;
    public float ElapsedTime;
    public float DamageMul;
    public float FireRateMul;
    public float RangeMul;
    [ReadOnly] public NativeParallelMultiHashMap<int2, EnemySpatialEntry> SpatialMap;
    public float3 SpatialOrigin;
    public float SpatialCellSize;
    public EntityCommandBuffer.ParallelWriter ECB;

    void Execute(
            [ChunkIndexInQuery] int chunkIdx,
            in LocalTransform turretTransform,
            ref MortarStats stats,
            in MortarTag _)
    {
        stats.Cooldown -= DeltaTime;
        if (stats.Cooldown > 0f) return;

        int2 centerCell = SpatialIndexUtility.WorldToCell(
            turretTransform.Position, SpatialOrigin, SpatialCellSize);

        int cellRadius = (int)math.ceil(stats.Range * RangeMul / SpatialCellSize);

        float bestDistSq = stats.Range * stats.Range;
        float3 bestPos = float3.zero;
        bool found = false;

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
                        bestPos = entry.Position;
                        found = true;
                    }
                }
                while (SpatialMap.TryGetNextValue(out entry, ref it));
            }
        }

        if (!found) return;

        var aoeEntity = ECB.CreateEntity(chunkIdx);
        ECB.AddComponent(chunkIdx, aoeEntity, new DelayedAoE
        {
            Position = bestPos,
            Radius = stats.AoERadius,
            Damage = stats.Damage * DamageMul,
            ExplodeTime = ElapsedTime + stats.ExplodeDelay,
        });

        var markerEvent = ECB.CreateEntity(chunkIdx);
        ECB.AddComponent(chunkIdx, markerEvent, new ExplosionFiredEvent
        {
            Position = bestPos,
            Radius = stats.AoERadius,
            VfxId = VfxId.MortarPredictionMarker,
            SpawnTime = ElapsedTime,
        });

        stats.Cooldown = 1f / (stats.FireRate * FireRateMul);
    }
}