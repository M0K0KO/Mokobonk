using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(InfluenceGridUpdateSystem))]
partial struct ResourceCollectionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CoreInfluenceGridSingleton>();
        state.RequireForUpdate<ResourceSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gridCfg = SystemAPI.GetSingleton<GridConfigSingleton>();
        var inf = SystemAPI.GetSingleton<CoreInfluenceGridSingleton>();
        var resRW = SystemAPI.GetSingletonRW<ResourceSingleton>();
        float now = (float)SystemAPI.Time.ElapsedTime;

        int totalGold = 0;

        foreach (var (node, stateRef, transform) in
                 SystemAPI.Query<RefRO<ResourceNode>, RefRW<ResourceNodeState>, RefRO<LocalTransform>>())
        {
            if (now < stateRef.ValueRO.NextTickTime) continue;

            int2 cell = GridUtility.WorldToCell(transform.ValueRO.Position, gridCfg.Origin, gridCfg.CellSize);
            if (!inf.Cells.Contains(cell)) continue;

            totalGold += node.ValueRO.YieldPerTick;
            stateRef.ValueRW.NextTickTime = now + node.ValueRO.TickInterval;
        }

        if (totalGold > 0)
            resRW.ValueRW.Gold += totalGold;
    }
}