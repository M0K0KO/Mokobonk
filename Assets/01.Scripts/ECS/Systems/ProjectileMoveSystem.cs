using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(TurretTargetingSystem))]
public partial struct ProjectileMoveSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        new ProjectileMoveJob { DeltaTime = dt }.ScheduleParallel();
    }
}

//[BurstCompile]
public partial struct ProjectileMoveJob : IJobEntity
{
    public float DeltaTime;

    void Execute(ref LocalTransform transform, in ProjectileVelocity vel, in ProjectileTag _)
    {
        transform.Position += DeltaTime * vel.Speed * vel.Direction;

        UnityEngine.Debug.Log($"Projectile pos: {transform.Position}, vel.Speed: {vel.Speed}");
    }
}