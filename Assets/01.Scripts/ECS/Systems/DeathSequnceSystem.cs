using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine.UIElements;

partial struct DeathSequnceSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float now = (float)SystemAPI.Time.ElapsedTime;
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                           .CreateCommandBuffer(state.WorldUnmanaged)
                           .AsParallelWriter();

        new StartDyingJob { Now = now, ECB = ecb }.ScheduleParallel();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }

    const int DieClipIndex = 1;
    const float DieBlendDuration = 0.15f;
    const float DespawnDelay = 1.0f;

    [BurstCompile]
    [WithNone(typeof(DyingTag))]
    partial struct StartDyingJob : IJobEntity
    {
        public float Now;
        public EntityCommandBuffer.ParallelWriter ECB;

        void Execute(
            [ChunkIndexInQuery] int chunkIndex,
            Entity entity,
            in LocalTransform transform,
            in Health health,
            in VATAnimationState animState)
        {
            if (health.Current > 0) return;

            ECB.AddComponent(chunkIndex, entity, new DyingTag
            {
                DespawnTime = Now + DespawnDelay,
            });

            var evtEntity = ECB.CreateEntity(chunkIndex);
            ECB.AddComponent(chunkIndex, evtEntity, new EnemyKilledEvent
            {
                Position = transform.Position,
                EnemyType = 0
            });

            VATAnimationCommands.PlayClip(
                ECB, chunkIndex, entity,
                animState,
                newClipIndex: DieClipIndex,
                blendDuration: DieBlendDuration,
                currentTime: Now);

            ECB.SetComponent(chunkIndex, entity, new PhysicsVelocity
            {
                Linear = float3.zero,
                Angular = float3.zero
            });
        }
    }
}


