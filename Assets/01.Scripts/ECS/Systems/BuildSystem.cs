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
            if (!GridUtility.AreAllCellsFree(cmd.Cell, info.Size, grid.GridSize, occupancy))
                continue;

            if (resRW.ValueRO.Gold < info.Cost) continue;
            resRW.ValueRW.Gold -= info.Cost;

            var built = em.Instantiate(info.Prefab);

            float3 pos = GridUtility.FootprintCenterWorld(cmd.Cell, info.Size, grid.Origin, grid.CellSize);
            var existingTransform = em.GetComponentData<LocalTransform>(built);
            em.SetComponentData(built, new LocalTransform
            {
                Position = pos,
                Rotation = existingTransform.Rotation,
                Scale = existingTransform.Scale,
            });

            em.SetComponentData(built, new BuildableFootprint { Anchor = cmd.Cell, Size = info.Size });
            em.AddComponentData(built, new BuildableHealth
            {
                Current = info.MaxHealth,
                Max = info.MaxHealth
            });

            for (int dy = 0; dy < info.Size.y; dy++)
            {
                for (int dx = 0; dx < info.Size.x; dx++)
                {
                    int2 cell = new int2(cmd.Cell.x + dx, cmd.Cell.y + dy);
                    occupancy.TryAdd(cell, built);
                }
            }

            if (info.BlocksMovement) anyBlockingBuilt = true;
        }

        if (anyBlockingBuilt && SystemAPI.HasSingleton<FlowFieldDirtyFlag>())
        {
            SystemAPI.SetSingleton(new FlowFieldDirtyFlag { Value = true });
        }
    }
}