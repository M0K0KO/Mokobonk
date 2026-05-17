
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(TurretBuildSystem))]
public partial struct WallBuildSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnWallQueueSingleton>();
        state.RequireForUpdate<GridOccupancySingleton>();
        state.RequireForUpdate<GridConfigSingleton>();
        state.RequireForUpdate<WallSpawnConfigSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.CompleteDependency();

        var queue = SystemAPI.GetSingleton<SpawnWallQueueSingleton>().Queue;
        if (queue.Count == 0) return;

        var occupancy = SystemAPI.GetSingleton<GridOccupancySingleton>().Map;
        var grid = SystemAPI.GetSingleton<GridConfigSingleton>();
        var prefab = SystemAPI.GetSingleton<WallSpawnConfigSingleton>().WallPrefab;
        var em = state.EntityManager;

        bool anyBuilt = false;
        while (queue.TryDequeue(out var cmd))
        {
            if (occupancy.ContainsKey(cmd.Cell)) continue;
            if (!GridUtility.IsInBounds(cmd.Cell, grid.GridSize)) continue;

            var wall = em.Instantiate(prefab);
            float3 pos = GridUtility.CellToWorld(cmd.Cell, grid.Origin, grid.CellSize);

            em.SetComponentData(wall, LocalTransform.FromPosition(pos));
            em.AddComponentData(wall, new WallGridCell { Value = cmd.Cell });

            occupancy.TryAdd(cmd.Cell, wall);
            anyBuilt = true;
        }

        /*
        if (anyBuilt && SystemAPI.HasSingleton<FlowFieldDirtyFlag>())
        {
            SystemAPI.SetSingleton(new FlowFieldDirtyFlag { Value = true });
        }
        */
    }
}