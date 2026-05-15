using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
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
            DeltaTime = dt
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

    private void Execute(ref LocalTransform transform, in MoveSpeed moveSpeed)
    {
        float3 toPlayer = PlayerPos - transform.Position;
        toPlayer.y = 0;
        float3 dir = math.normalizesafe(toPlayer);

        transform.Position += dir * moveSpeed.Speed * DeltaTime;
    }
}
