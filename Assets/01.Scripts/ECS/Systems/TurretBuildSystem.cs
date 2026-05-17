using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct TurretBuildSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnTurretQueueSingleton>();
        state.RequireForUpdate<GridOccupancySingleton>();
        state.RequireForUpdate<GridConfigSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.CompleteDependency();

        var queue = SystemAPI.GetSingleton<SpawnTurretQueueSingleton>().Queue;
        if (queue.Count == 0) return;

        var occupancy = SystemAPI.GetSingleton<GridOccupancySingleton>().Map;
        var grid = SystemAPI.GetSingleton<GridConfigSingleton>();
        var prefab = SystemAPI.GetSingleton<TurretSpawnConfigSingleton>().TurretPrefab;
        var em = state.EntityManager;

        bool anyBuilt = false;
        while (queue.TryDequeue(out var cmd))
        {
            if (occupancy.ContainsKey(cmd.Cell)) continue;
            if (!GridUtility.IsInBounds(cmd.Cell, grid.GridSize)) continue;

            var turret = em.Instantiate(prefab);
            float3 pos = GridUtility.CellToWorld(cmd.Cell, grid.Origin, grid.CellSize);

            em.SetComponentData(turret, LocalTransform.FromPosition(pos));
            em.AddComponentData(turret, new TurretGridCell { Value = cmd.Cell });

            occupancy.TryAdd(cmd.Cell, turret);

            anyBuilt = true;
        }

        if (anyBuilt && SystemAPI.HasSingleton<FlowFieldDirtyFlag>())
        {
            SystemAPI.SetSingleton(new FlowFieldDirtyFlag { Value = true });
        }
    }
}