using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
public partial struct EventCleanupSystem : ISystem
{
    private EntityQuery _killedQuery;

    public void OnCreate(ref SystemState state)
    {
        _killedQuery = state.GetEntityQuery(typeof(EnemyKilledEvent));
    }

    public void OnUpdate(ref SystemState state)
    {
        state.EntityManager.DestroyEntity(_killedQuery);
    }
}