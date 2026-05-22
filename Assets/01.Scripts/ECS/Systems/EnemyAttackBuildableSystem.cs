using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct EnemyAttackBuildableSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridOccupancySingleton>();
        state.RequireForUpdate<GridConfigSingleton>();
        state.RequireForUpdate<BalanceMultiplierSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var occ = SystemAPI.GetSingleton<GridOccupancySingleton>();
        var grid = SystemAPI.GetSingleton<GridConfigSingleton>();
        var bal = SystemAPI.GetSingleton<BalanceMultiplierSingleton>();

        var healthLookup = SystemAPI.GetComponentLookup<BuildableHealth>(false);
        var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

        state.CompleteDependency();

        state.Dependency = new EnemyAttackJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            DamageMul = bal.EnemyDamageMul,
            Occupancy = occ.Map,
            GridOrigin = grid.Origin,
            GridCellSize = grid.CellSize,
            HealthLookup = healthLookup,
            TransformLookup = transformLookup,
        }.ScheduleParallel(state.Dependency);
    }
}

//[BurstCompile]
public partial struct EnemyAttackJob : IJobEntity
{
    public float DeltaTime;
    public float DamageMul;

    [ReadOnly] public NativeHashMap<int2, Entity> Occupancy;
    public float3 GridOrigin;
    public float GridCellSize;

    [NativeDisableParallelForRestriction]
    public ComponentLookup<BuildableHealth> HealthLookup;
    [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;

    void Execute(in LocalTransform enemyTransform, ref EnemyAttackStats stats)
    {
        UnityEngine.Debug.Log($"[Attack] tick pos={enemyTransform.Position}");
        stats.Cooldown -= DeltaTime;
        if (stats.Cooldown > 0f) return;

        int2 centerCell = GridUtility.WorldToCell(enemyTransform.Position, GridOrigin, GridCellSize);
        int cellRadius = (int)math.ceil(stats.Range / GridCellSize);

        UnityEngine.Debug.Log($"[Attack] scan centerCell={centerCell} radius={(int)math.ceil(stats.Range / GridCellSize)}");

        Entity bestTarget = Entity.Null;
        float bestDistSq = stats.Range * stats.Range;

        for (int dy = -cellRadius; dy <= cellRadius; dy++)
        {
            for (int dx = -cellRadius; dx <= cellRadius; dx++)
            {
                int2 cell = centerCell + new int2(dx, dy);
                if (!Occupancy.TryGetValue(cell, out var occupant)) continue;
                UnityEngine.Debug.Log($"[Attack] FOUND cell={cell} entity={occupant.Index} hasHealth={HealthLookup.HasComponent(occupant)}");
                if (!HealthLookup.HasComponent(occupant)) continue;

                float3 occPos = TransformLookup[occupant].Position;
                float3 d = occPos - enemyTransform.Position;
                d.y = 0f;
                float distSq = math.lengthsq(d);

                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    bestTarget = occupant;
                }
            }
        }

        if (bestTarget == Entity.Null) return;

        var hp = HealthLookup[bestTarget];
        hp.Current -= stats.Damage * DamageMul;
        HealthLookup[bestTarget] = hp;

        stats.Cooldown = stats.Interval;
    }
}