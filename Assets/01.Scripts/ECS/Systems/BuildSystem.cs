using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct BuildSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnBuildQueueSingleton>();
        state.RequireForUpdate<BuildableRegistrySingleton>();
        state.RequireForUpdate<GridOccupancySingleton>();
        state.RequireForUpdate<GridConfigSingleton>();
        state.RequireForUpdate<ResourceSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.CompleteDependency();

        var queue = SystemAPI.GetSingleton<SpawnBuildQueueSingleton>().Queue;
        if (queue.Count == 0) return;

        var registry = SystemAPI.GetSingleton<BuildableRegistrySingleton>();
        var occupancy = SystemAPI.GetSingleton<GridOccupancySingleton>().Map;
        var grid = SystemAPI.GetSingleton<GridConfigSingleton>();
        var resRW = SystemAPI.GetSingletonRW<ResourceSingleton>();
        var em = state.EntityManager;

        bool anyBlockingBuilt = false;

        while (queue.TryDequeue(out var cmd))
        {
            if (!registry.TryGet(cmd.Kind, out var info)) continue;
            if (!GridUtility.IsInBounds(cmd.Cell, grid.GridSize)) continue;
            if (occupancy.ContainsKey(cmd.Cell)) continue;
            if (resRW.ValueRO.Gold < info.Cost) continue;
            resRW.ValueRW.Gold -= info.Cost;

            var built = em.Instantiate(info.Prefab);
            float3 pos = GridUtility.CellToWorld(cmd.Cell, grid.Origin, grid.CellSize);
            var existingTransform = em.GetComponentData<LocalTransform>(built);
            em.SetComponentData(built, new LocalTransform
            {
                Position = pos,
                Rotation = existingTransform.Rotation,
                Scale = existingTransform.Scale,
            });

            em.AddComponentData(built, new BuildGridCell { Value = cmd.Cell });

            occupancy.TryAdd(cmd.Cell, built);

            if (info.BlocksMovement) anyBlockingBuilt = true;
        }

        if (anyBlockingBuilt && SystemAPI.HasSingleton<FlowFieldDirtyFlag>())
        {
            SystemAPI.SetSingleton(new FlowFieldDirtyFlag { Value = true });
        }
    }
}