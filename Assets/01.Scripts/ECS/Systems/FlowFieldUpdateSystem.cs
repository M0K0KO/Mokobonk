using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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
        var healthLookup = SystemAPI.GetComponentLookup<BuildableHealth>(true);

        int2 coreCell = GridUtility.WorldToCell(corePos, grid.Origin, grid.CellSize);

        int total = field.Width * field.Height;
        var staticCosts = new NativeArray<ushort>(total, Allocator.Temp);
        for (int i = 0; i < total; i++) staticCosts[i] = 1;

        const float costScale = 0.5f;
        foreach (var kv in occupancy)
        {
            int2 cell = kv.Key;
            if (!GridUtility.IsInBounds(cell, grid.GridSize)) continue;
            Entity entity = kv.Value;
            int idx = CellToIndex(cell, field.Width);

            if (healthLookup.HasComponent(entity))
            {
                var hp = healthLookup[entity];
                int costInt = (int)math.round(hp.Max * costScale);
                staticCosts[idx] = (ushort)math.clamp(costInt, 1, 65000);
            }
            else
            {
                staticCosts[idx] = ushort.MaxValue;
            }
        }

        for (int i = 0; i < field.Costs.Length; i++)
            field.Costs[i] = ushort.MaxValue;

        if (!GridUtility.IsInBounds(coreCell, grid.GridSize))
        {
            staticCosts.Dispose();
            return;
        }

        const int bucketCount = 4096;
        var buckets = new NativeArray<UnsafeList<int2>>(bucketCount, Allocator.Temp);
        for (int i = 0; i < bucketCount; i++)
            buckets[i] = new UnsafeList<int2>(16, Allocator.Temp);

        field.Costs[CellToIndex(coreCell, field.Width)] = 0;
        var b0 = buckets[0];
        b0.Add(coreCell);
        buckets[0] = b0;

        var neighbors4 = new NativeArray<int2>(4, Allocator.Temp);
        neighbors4[0] = new int2(1, 0);
        neighbors4[1] = new int2(-1, 0);
        neighbors4[2] = new int2(0, 1);
        neighbors4[3] = new int2(0, -1);

        int currentBucket = 0;
        while (currentBucket < bucketCount)
        {
            var bucket = buckets[currentBucket];
            if (bucket.Length == 0)
            {
                currentBucket++;
                continue;
            }

            int2 cur = bucket[bucket.Length - 1];
            bucket.RemoveAt(bucket.Length - 1);
            buckets[currentBucket] = bucket;

            ushort curCost = field.Costs[CellToIndex(cur, field.Width)];
            if (curCost < currentBucket) continue;

            for (int i = 0; i < 4; i++)
            {
                int2 next = cur + neighbors4[i];
                if (!GridUtility.IsInBounds(next, grid.GridSize)) continue;

                int nextIdx = CellToIndex(next, field.Width);
                ushort enterCost = staticCosts[nextIdx];
                if (enterCost == ushort.MaxValue) continue;

                int newCostInt = curCost + enterCost;
                if (newCostInt >= ushort.MaxValue || newCostInt >= bucketCount) continue;

                ushort newCost = (ushort)newCostInt;
                if (newCost < field.Costs[nextIdx])
                {
                    field.Costs[nextIdx] = newCost;
                    var b = buckets[newCost];
                    b.Add(next);
                    buckets[newCost] = b;
                }
            }
        }

        var neighbors8 = new NativeArray<int2>(8, Allocator.Temp);
        neighbors8[0] = new int2(1, 0);
        neighbors8[1] = new int2(-1, 0);
        neighbors8[2] = new int2(0, 1);
        neighbors8[3] = new int2(0, -1);
        neighbors8[4] = new int2(1, 1);
        neighbors8[5] = new int2(-1, 1);
        neighbors8[6] = new int2(1, -1);
        neighbors8[7] = new int2(-1, -1);

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
                        int sideAIdx = CellToIndex(sideA, field.Width);
                        int sideBIdx = CellToIndex(sideB, field.Width);
                        if (staticCosts[sideAIdx] == ushort.MaxValue ||
                            staticCosts[sideBIdx] == ushort.MaxValue) continue;
                    }

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

        // dispose
        staticCosts.Dispose();
        for (int i = 0; i < bucketCount; i++) buckets[i].Dispose();
        buckets.Dispose();
        neighbors4.Dispose();
        neighbors8.Dispose();
    }

    private static int CellToIndex(int2 cell, int width) => cell.x + cell.y * width;
}