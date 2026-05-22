using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemySpawnSystem))]
[UpdateAfter(typeof(FlowFieldUpdateSystem))]
[UpdateAfter(typeof(SpatialIndexUpdateSystem))]
partial struct EnemyMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CorePositionSingleton>();
        state.RequireForUpdate<FlowFieldSingleton>();
        state.RequireForUpdate<GridConfigSingleton>();
        state.RequireForUpdate<SpatialIndexSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.CompleteDependency();

        var field = SystemAPI.GetSingleton<FlowFieldSingleton>();
        var grid = SystemAPI.GetSingleton<GridConfigSingleton>();
        var corePos = SystemAPI.GetSingleton<CorePositionSingleton>().Value;
        var spatial = SystemAPI.GetSingleton<SpatialIndexSingleton>();
        var bal = SystemAPI.GetSingleton<BalanceMultiplierSingleton>();

        new ChaseCoreJob
        {
            FlowDirections = field.Directions,
            FieldWidth = field.Width,
            FieldHeight = field.Height,
            GridOrigin = grid.Origin,
            CellSize = grid.CellSize,
            GridSize = grid.GridSize,
            CorePos = corePos,

            SpatialMap = spatial.Map,
            SpatialOrigin = spatial.Origin,
            SpatialCell = spatial.CellSize,

            SpeedMul = bal.EnemySpeedMul,

            DeltaTime = SystemAPI.Time.DeltaTime
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}


[BurstCompile]
[WithDisabled(typeof(AttackingTag))]
[WithNone(typeof(DyingTag))]
public partial struct ChaseCoreJob : IJobEntity
{
    [ReadOnly] public NativeArray<float3> FlowDirections;
    public int FieldWidth;
    public int FieldHeight;
    public float3 GridOrigin;
    public float CellSize;
    public int2 GridSize;
    public float3 CorePos;
    public float SpeedMul;

    [ReadOnly] public NativeParallelMultiHashMap<int2, EnemySpatialEntry> SpatialMap;
    public float3 SpatialOrigin;
    public float SpatialCell;

    public float DeltaTime;

    private const float SeparationRadius = 1.5f;
    private const float SeparationRadiusSq = SeparationRadius * SeparationRadius;
    private const float SeparationWeight = 0.8f;
    private const float VelocitySmoothing = 8f;

    private void Execute(
        Entity self,
        ref LocalTransform transform, 
        in MoveSpeed moveSpeed, 
        in RotateSpeed rotateSpeed, 
        ref PhysicsVelocity velocity, 
        in EnemyTag _)
    {
        var effectiveSpeed = moveSpeed.Speed * SpeedMul;

        int2 cell = GridUtility.WorldToCell(transform.Position, GridOrigin, CellSize);
        float3 flowDir = float3.zero;

        if (GridUtility.IsInBounds(cell, GridSize))
        {
            int idx = cell.x + cell.y * FieldWidth;
            flowDir = FlowDirections[idx];
        }

        if (math.lengthsq(flowDir) < 1e-4f)
        {
            velocity.Linear = float3.zero;
            transform.Position = new float3(transform.Position.x, 0.1f, transform.Position.z);
            return;
        }

        float3 separation = float3.zero;
        int2 spatialCenter = SpatialIndexUtility.WorldToCell(
            transform.Position, SpatialOrigin, SpatialCell);

        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int2 scell = spatialCenter + new int2(dx, dy);
                if (!SpatialMap.TryGetFirstValue(scell, out var entry, out var it)) continue;

                do
                {
                    if (entry.Entity == self) continue;

                    float3 away = transform.Position - entry.Position;
                    away.y = 0f;
                    float distSq = math.lengthsq(away);
                    if (distSq < 1e-4f || distSq > SeparationRadiusSq) continue;

                    separation += away / distSq;
                }
                while (SpatialMap.TryGetNextValue(out entry, ref it));
            }
        }

        float3 desiredDir = flowDir + separation * SeparationWeight;
        desiredDir.y = 0f;

        if (math.lengthsq(desiredDir) < 1e-4f)
        {
            desiredDir = flowDir;
        }
        else
        {
            desiredDir = math.normalize(desiredDir);
        }

        float3 targetVel = new float3(
            desiredDir.x * effectiveSpeed,
            0f,
            desiredDir.z * effectiveSpeed
        );

        velocity.Linear = math.lerp(
            velocity.Linear,
            targetVel,
            math.saturate(DeltaTime * VelocitySmoothing)
        );

        float3 lookDir = velocity.Linear;
        lookDir.y = 0f;
        if (math.lengthsq(lookDir) > math.EPSILON)
        {
            quaternion targetRot = quaternion.LookRotationSafe(math.normalize(lookDir), math.up());
            transform.Rotation = math.slerp(
                transform.Rotation,
                targetRot,
                math.saturate(DeltaTime * rotateSpeed.Speed)
            );
        }
        transform.Position = new float3(transform.Position.x, 0.1f, transform.Position.z);
    }
}
