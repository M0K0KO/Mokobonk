using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct InfluenceGridUpdateSystem : ISystem
{
    private EntityQuery _providerQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CoreInfluenceGridSingleton>();
        _providerQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<InfluenceProvider>(),
            ComponentType.ReadOnly<LocalTransform>());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gridCfg = SystemAPI.GetSingleton<GridConfigSingleton>();
        var influenceRW = SystemAPI.GetSingletonRW<CoreInfluenceGridSingleton>();
        ref var inf = ref influenceRW.ValueRW;

        inf.Cells.Clear();

        var providers = _providerQuery.ToComponentDataArray<InfluenceProvider>(Allocator.Temp);
        var transforms = _providerQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

        for (int i = 0; i < providers.Length; i++)
        {
            var pos = transforms[i].Position;
            FillCircle(ref inf, ref pos, providers[i].Radius, gridCfg);
        }

        providers.Dispose();
        transforms.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        if (SystemAPI.HasSingleton<CoreInfluenceGridSingleton>())
        {
            var grid = SystemAPI.GetSingleton<CoreInfluenceGridSingleton>();
            if (grid.Cells.IsCreated) grid.Cells.Dispose();
        }
    }


    [BurstCompile]
    private static void FillCircle(ref CoreInfluenceGridSingleton inf, ref float3 center, float radius, in GridConfigSingleton cfg)
    {
        int2 centerCell = GridUtility.WorldToCell(center, cfg.Origin, cfg.CellSize);
        int cellRadius = (int)math.ceil(radius / cfg.CellSize);
        float radiusSq = radius * radius;

        for (int dy = -cellRadius; dy <= cellRadius; dy++)
        {
            for (int dx = -cellRadius; dx <= cellRadius; dx++)
            {
                int2 cell = centerCell + new int2(dx, dy);
                if (!GridUtility.IsInBounds(cell, cfg.GridSize)) continue;

                float3 cellWorld = GridUtility.CellToWorld(cell, cfg.Origin, cfg.CellSize);
                float3 d = cellWorld - center;
                d.y = 0f;
                if (math.lengthsq(d) <= radiusSq)
                    inf.Cells.Add(cell);
            }
        }
    }
}