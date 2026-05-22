using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyAttackBuildableSystem))]
public partial struct BuildableDeathSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridOccupancySingleton>();
        state.RequireForUpdate<GridConfigSingleton>();
        state.RequireForUpdate<FlowFieldDirtyFlag>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<BuildableRegistrySingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        var occupancy = SystemAPI.GetSingleton<GridOccupancySingleton>();
        var registry = SystemAPI.GetSingleton<BuildableRegistrySingleton>();
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        var flowFieldDirty = SystemAPI.GetSingletonRW<FlowFieldDirtyFlag>();
        var gridConfig = SystemAPI.GetSingleton<GridConfigSingleton>();

        var now = (float)SystemAPI.Time.ElapsedTime;

        bool anyDeath = false;
        
        foreach(var (health, footprint, transform, entity) in SystemAPI.Query<RefRO<BuildableHealth>, RefRO<BuildableFootprint>, RefRO <LocalTransform>>().WithEntityAccess())
        {
            if (health.ValueRO.Current > 0f) continue;

            int2 anchor = footprint.ValueRO.Anchor;
            int2 size = footprint.ValueRO.Size;

            for (int dy = 0; dy < size.y; dy++)
            {
                for (int dx = 0; dx < size.x; dx++)
                {
                    occupancy.Map.Remove(new int2(anchor.x + dx, anchor.y + dy));
                }
            }

            var evt = ecb.CreateEntity();
            ecb.AddComponent(evt, new BuildableDeathEvent
            {
                Position = transform.ValueRO.Position,
                SpawnTime = now,
                VfxId = VfxId.BuildableDestroyed
            });

            ecb.DestroyEntity(entity);
            anyDeath = true;
        }

        if (anyDeath)
            flowFieldDirty.ValueRW.Value = true;
    }
}