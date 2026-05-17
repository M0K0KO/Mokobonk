using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(EnemyMovementSystem))]
public partial struct FlowFieldUpdateSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FlowFieldSingleton>();
        state.RequireForUpdate<FlowFieldDirtyFlag>();
        state.RequireForUpdate<GridOccupancySingleton>();
        state.RequireForUpdate<GridConfigSingleton>();
        state.RequireForUpdate<CorePositionSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var dirtyRW = SystemAPI.GetSingletonRW<FlowFieldDirtyFlag>();
        if (!dirtyRW.ValueRO.Value) return;
        dirtyRW.ValueRW.Value = false;

        state.CompleteDependency();

        var field = SystemAPI.GetSingleton<FlowFieldSingleton>();
        var occupancy = SystemAPI.GetSingleton<GridOccupancySingleton>().Map;
        var grid = SystemAPI.GetSingleton<GridConfigSingleton>();
        var corePos = SystemAPI.GetSingleton<CorePositionSingleton>().Value;

        int2 coreCell = GridUtility.WorldToCell(corePos, grid.Origin, grid.CellSize);

        for (int i = 0; i < field.Costs.Length; i++)
            field.Costs[i] = ushort.MaxValue;

        if (!GridUtility.IsInBounds(coreCell, grid.GridSize)) return;

        var queue = new NativeQueue<int2>(Allocator.Temp);
        queue.Enqueue(coreCell);
        field.Costs[CellToIndex(coreCell, field.Width)] = 0;

        var neighbors8 = new NativeArray<int2>(8, Allocator.Temp);
        neighbors8[0] = new int2(1, 0);   // E
        neighbors8[1] = new int2(-1, 0);   // W
        neighbors8[2] = new int2(0, 1);   // N
        neighbors8[3] = new int2(0, -1);   // S
        neighbors8[4] = new int2(1, 1);   // NE
        neighbors8[5] = new int2(-1, 1);   // NW
        neighbors8[6] = new int2(1, -1);   // SE
        neighbors8[7] = new int2(-1, -1);   // SW

        while (queue.TryDequeue(out int2 cur))
        {
            ushort curCost = field.Costs[CellToIndex(cur, field.Width)];

            for (int i = 0; i < 4; i++)
            {
                int2 next = cur + neighbors8[i];
                if (!GridUtility.IsInBounds(next, grid.GridSize)) continue;

                int idx = CellToIndex(next, field.Width);

                if (occupancy.ContainsKey(next)) continue;

                ushort newCost = (ushort)(curCost + 1);
                if (newCost < field.Costs[idx])
                {
                    field.Costs[idx] = newCost;
                    queue.Enqueue(next);
                }
            }
        }

        for (int y = 0; y < field.Height; y++)
        {
            for (int x = 0; x < field.Width; x++)
            {
                int2 cell = new int2(x, y);
                int idx = CellToIndex(cell, field.Width);

                if (field.Costs[idx] == ushort.MaxValue || cell.Equals(coreCell))
                {
                    field.Directions[idx] = float3.zero;
                    continue;
                }

                ushort bestCost = field.Costs[idx];
                int2 bestDir = int2.zero;
                bool found = false;

                for (int i = 0; i < 8; i++)
                {
                    int2 next = cell + neighbors8[i];
                    if (!GridUtility.IsInBounds(next, grid.GridSize)) continue;

                    if (i >= 4)
                    {
                        int2 sideA = new int2(cell.x + neighbors8[i].x, cell.y);
                        int2 sideB = new int2(cell.x, cell.y + neighbors8[i].y);
                        if (occupancy.ContainsKey(sideA) || occupancy.ContainsKey(sideB)) continue;
                    }

                    if (occupancy.ContainsKey(next)) continue;

                    ushort nCost = field.Costs[CellToIndex(next, field.Width)];
                    if (nCost < bestCost)
                    {
                        bestCost = nCost;
                        bestDir = neighbors8[i];
                        found = true;
                    }
                }

                field.Directions[idx] = found
                    ? math.normalize(new float3(bestDir.x, 0f, bestDir.y))
                    : float3.zero;
            }
        }

        queue.Dispose();
        neighbors8.Dispose();
    }

    private static int CellToIndex(int2 cell, int width) => cell.x + cell.y * width;
}