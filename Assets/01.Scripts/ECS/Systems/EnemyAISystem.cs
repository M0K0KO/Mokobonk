using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemySpawnSystem))]
partial struct EnemyAISystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CorePositionSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float3 corePos = SystemAPI.GetSingleton<CorePositionSingleton>().Value;
        float dt = SystemAPI.Time.DeltaTime;

        new ChaseCoreJob { CorePos = corePos, DeltaTime = dt }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}


[BurstCompile]
public partial struct ChaseCoreJob : IJobEntity
{
    public float3 CorePos;
    public float DeltaTime;

    private void Execute(ref LocalTransform transform, in MoveSpeed moveSpeed, in RotateSpeed rotateSpeed, ref PhysicsVelocity velocity, in EnemyTag _)
    {
        float3 toCore = CorePos - transform.Position;
        toCore.y = 0f;

        float distSq = math.lengthsq(toCore);
        if (distSq < 1e-4f) { velocity.Linear = float3.zero; return; }

        float3 dir = toCore * math.rsqrt(distSq);
        velocity.Linear = new float3(dir.x * moveSpeed.Speed, 0f, dir.z * moveSpeed.Speed);

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
