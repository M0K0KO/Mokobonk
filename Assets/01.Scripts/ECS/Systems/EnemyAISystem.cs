using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
partial struct EnemyAISystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerPositionSingleton>();
        state.RequireForUpdate<EnemyTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var playerPos = SystemAPI.GetSingleton<PlayerPositionSingleton>().Position;
        var dt = SystemAPI.Time.DeltaTime;

        new MoveEnemyJob
        {
            PlayerPos = playerPos,
            DeltaTime = dt,
        }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}


[BurstCompile]
[WithAll(typeof(EnemyTag))]
public partial struct MoveEnemyJob : IJobEntity
{
    public float3 PlayerPos;
    public float DeltaTime;
    private void Execute(ref LocalTransform transform, in MoveSpeed moveSpeed, in RotateSpeed rotateSpeed, ref PhysicsVelocity velocity, in EnemyTag _)
    {
        float3 toPlayer = PlayerPos - transform.Position;
        toPlayer.y = 0;

        float3 dir = math.normalizesafe(toPlayer);

        velocity.Linear = dir * moveSpeed.Speed;

        if (math.lengthsq(dir) > math.EPSILON)
        {
            quaternion targetRot = quaternion.LookRotationSafe(dir, math.up());

            transform.Rotation = math.slerp(
                transform.Rotation,
                targetRot,
                math.saturate(DeltaTime * rotateSpeed.Speed)
            );
        }
    }
}
