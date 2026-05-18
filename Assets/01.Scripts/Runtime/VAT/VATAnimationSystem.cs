using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct VATAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float now = (float)SystemAPI.Time.ElapsedTime;
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                            .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        new TransitionJob { Now = now, ECB = ecb }.ScheduleParallel();
    }

    [BurstCompile]
    partial struct TransitionJob : IJobEntity
    {
        public float Now;
        public EntityCommandBuffer.ParallelWriter ECB;

        void Execute(
            [Unity.Entities.ChunkIndexInQuery] int chunkIndex,
            Entity entity,
            in VATAnimationTransition trans)
        {
            float elapsed = Now - trans.TransitionStartTime;
            if (elapsed >= trans.TransitionDuration)
            {
                ECB.RemoveComponent<VATAnimationTransition>(chunkIndex, entity);
            }
        }
    }
}