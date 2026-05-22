using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static UnityEditor.PlayerSettings;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct EnemyAttackBuildableSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridOccupancySingleton>();
        state.RequireForUpdate<GridConfigSingleton>();
        state.RequireForUpdate<BalanceMultiplierSingleton>();
        state.RequireForUpdate<FlowFieldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var occ = SystemAPI.GetSingleton<GridOccupancySingleton>();
        var grid = SystemAPI.GetSingleton<GridConfigSingleton>();
        var bal = SystemAPI.GetSingleton<BalanceMultiplierSingleton>();
        var field = SystemAPI.GetSingleton<FlowFieldSingleton>();

        var healthLookup = SystemAPI.GetComponentLookup<BuildableHealth>(false);

        state.CompleteDependency();

        state.Dependency = new EnemyAttackJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            DamageMul = bal.EnemyDamageMul,
            Occupancy = occ.Map,
            GridOrigin = grid.Origin,
            GridCellSize = grid.CellSize,
            HealthLookup = healthLookup,
            FlowCosts = field.Costs,
            FieldWidth = field.Width,
            FieldHeight = field.Height,
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithPresent(typeof(AttackingTag))]
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

    [ReadOnly] public NativeArray<ushort> FlowCosts;
    public int FieldWidth;
    public int FieldHeight;


    void Execute(in LocalTransform enemyTransform, ref EnemyAttackStats stats, ref EnemyAttackTarget target, EnabledRefRW<AttackingTag> attacking)
    {
        stats.Cooldown -= DeltaTime;

        int2 centerCell = GridUtility.WorldToCell(enemyTransform.Position, GridOrigin, GridCellSize);

        if (centerCell.x < 0 || centerCell.x >= FieldWidth ||
            centerCell.y < 0 || centerCell.y >= FieldHeight)
        {
            target.Value = Entity.Null;
            attacking.ValueRW = false;
            return;
        }

        int cellRadius = (int)math.ceil(stats.Range / GridCellSize);
        ushort myCost = FlowCosts[centerCell.x + centerCell.y * FieldWidth];
        float rangeSq = stats.Range * stats.Range;


        Entity bestCandidate = Entity.Null;
        float bestCandidateDistSq = rangeSq;
        bool currentTargetStillInRange = false;

        for (int dy = -cellRadius; dy <= cellRadius; dy++)
        { 
            for (int dx = -cellRadius; dx <= cellRadius; dx++)
            {
                int2 cell = centerCell + new int2(dx, dy);

                if (cell.x < 0 || cell.x >= FieldWidth || cell.y < 0 || cell.y >= FieldHeight)
                    continue;

                if (!Occupancy.TryGetValue(cell, out var occupant)) continue;
                if (!HealthLookup.HasComponent(occupant)) continue;

                float3 cellCenter = GridUtility.CellToWorld(cell, GridOrigin, GridCellSize);
                float3 d = cellCenter - enemyTransform.Position;
                d.y = 0f;
                float distSq = math.lengthsq(d);
                if (distSq > rangeSq) continue;

                if (occupant == target.Value)
                    currentTargetStillInRange = true;

                ushort wallCost = FlowCosts[cell.x + cell.y * FieldWidth];
                if (wallCost >= myCost) continue;

                if (distSq < bestCandidateDistSq)
                {
                    bestCandidateDistSq = distSq;
                    bestCandidate = occupant;
                }
            }
        }

        if (!currentTargetStillInRange)
            target.Value = bestCandidate;

        bool hasTarget = target.Value != Entity.Null;
        attacking.ValueRW = hasTarget;
        if (!hasTarget) return;

        if (stats.Cooldown > 0f) return;

        var hp = HealthLookup[target.Value];
        hp.Current -= stats.Damage * DamageMul;
        HealthLookup[target.Value] = hp;
        stats.Cooldown = stats.Interval;
    }
}