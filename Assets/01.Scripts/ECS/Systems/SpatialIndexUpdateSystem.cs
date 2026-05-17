using System.Net.NetworkInformation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct SpatialIndexUpdateSystem : ISystem
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

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var idx = SystemAPI.GetSingleton<SpatialIndexSingleton>();

        state.CompleteDependency();

        int enemyCount = _enemyQuery.CalculateEntityCount();

        if (idx.Map.Capacity < enemyCount)
        {
            idx.Map.Capacity = math.max(enemyCount * 2, 1024);
        }

        idx.Map.Clear();

        new BuildSpatialIndexJob
        {
            Writer = idx.Map.AsParallelWriter(),
            CellSize = idx.CellSize,
            Origin = idx.Origin
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct BuildSpatialIndexJob : IJobEntity
{
    public NativeParallelMultiHashMap<int2, EnemySpatialEntry>.ParallelWriter Writer;
    public float CellSize;
    public float3 Origin;

    void Execute(Entity entity, in LocalTransform transform, in EnemyTag _)
    {
        int2 cell = SpatialIndexUtility.WorldToCell(transform.Position, Origin, CellSize);
        Writer.Add(cell, new EnemySpatialEntry
        {
            Entity = entity,
            Position = transform.Position
        });
    }
}