using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct ResourceRewardSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ResourceSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var resourceRW = SystemAPI.GetSingletonRW<ResourceSingleton>();
        int goldPerKill = 5; // will be replaced with balance sheet

        foreach(var evt in SystemAPI.Query<RefRO<EnemyKilledEvent>>())
        {
            resourceRW.ValueRW.Gold += goldPerKill;
        }
    }
}