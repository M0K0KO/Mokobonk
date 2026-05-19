using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(WaveSystem))]
partial struct PhaseProjectionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WaveStateSingleton>();
        state.RequireForUpdate<GamePhaseSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var wave = SystemAPI.GetSingleton<WaveStateSingleton>();
        var phaseRW = SystemAPI.GetSingletonRW<GamePhaseSingleton>();
        ref var phase = ref phaseRW.ValueRW;

        var newPhase = wave.Phase == WavePhase.Preparing
                          ? GamePhase.Preparation
                          : GamePhase.Horde;

        if (phase.Phase != newPhase)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                                .CreateCommandBuffer(state.WorldUnmanaged);

            var evt = ecb.CreateEntity();
            ecb.AddComponent(evt, new PhaseChangedEvent
            {
                From = phase.Phase,
                To = newPhase,
                WaveIndex = wave.CurrentWave,
            });

            phase.Phase = newPhase;
            phase.PhaseElapsed = 0f;
        }
        else
        {
            phase.PhaseElapsed += SystemAPI.Time.DeltaTime;
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
}