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
partial struct EnemyAISystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CorePositionSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var field = SystemAPI.GetSingleton<FlowFieldSingleton>();
        var grid = SystemAPI.GetSingleton<GridConfigSingleton>();
        var corePos = SystemAPI.GetSingleton<CorePositionSingleton>().Value;

        new ChaseCoreJob
        {
            FlowDirections = field.Directions,
            FieldWidth = field.Width,
            FieldHeight = field.Height,
            GridOrigin = grid.Origin,
            CellSize = grid.CellSize,
            GridSize = grid.GridSize,
            CorePos = corePos,
            DeltaTime = SystemAPI.Time.DeltaTime
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}


[BurstCompile]
public partial struct ChaseCoreJob : IJobEntity
{
    [ReadOnly] public NativeArray<float3> FlowDirections;
    public int FieldWidth;
    public int FieldHeight;
    public float3 GridOrigin;
    public float CellSize;
    public int2 GridSize;
    public float3 CorePos;
    public float DeltaTime;

    private void Execute(
        ref LocalTransform transform, 
        in MoveSpeed moveSpeed, 
        in RotateSpeed rotateSpeed, 
        ref PhysicsVelocity velocity, 
        in EnemyTag _)
    {
        int2 cell = GridUtility.WorldToCell(transform.Position, GridOrigin, CellSize);
        float3 dir;

        if (GridUtility.IsInBounds(cell, GridSize))
        {
            int idx = cell.x + cell.y * FieldWidth;
            dir = FlowDirections[idx];

            if (math.lengthsq(dir) < math.EPSILON)
            {
                velocity.Linear = float3.zero;
                return;
            }
        }
        else
        {
            float3 toCore = CorePos - transform.Position;
            toCore.y = 0f;
            float distSq = math.lengthsq(toCore);
            if (distSq < math.EPSILON) { velocity.Linear = float3.zero; return; }
            {
                dir =toCore * math.rsqrt(distSq);
            }
        }

        velocity.Linear = new float3(dir.x * moveSpeed.Speed, 0f, dir.z * moveSpeed.Speed);

        if (math.lengthsq(dir) > math.EPSILON)
        {
            quaternion targetRot = quaternion.LookRotationSafe(dir, math.up());
            transform.Rotation = math.slerp(
                transform.Rotation,
                targetRot,
                math.saturate(DeltaTime * rotateSpeed.Speed)
            );
        }
        transform.Position = new float3(transform.Position.x, 0.55f, transform.Position.z);
    }
}
