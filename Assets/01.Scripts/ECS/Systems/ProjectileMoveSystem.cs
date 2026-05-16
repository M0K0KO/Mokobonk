using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;


[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
partial struct ProjectileMoveSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WeaponState>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;
        var speed = SystemAPI.GetSingleton<WeaponState>().projectileSpeed;

        state.Dependency = new ProjectileMoveJob { dt = dt, speed = speed }.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}

[BurstCompile]
public partial struct ProjectileMoveJob : IJobEntity
{
    public float dt;
    public float speed;

    void Execute(ref LocalTransform transform, in ProjectileDirection dir, ref Lifetime lifetime, in ProjectileTag _)
    {
        transform.Position += dir.Value * speed * dt;
        lifetime.Remaining -= dt;
    }
}
