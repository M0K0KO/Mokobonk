using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[BurstCompile]
public partial struct VATSyncSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float now = (float)SystemAPI.Time.ElapsedTime;
        new SyncStaticJob().ScheduleParallel();
        new SyncTransitionJob { Now = now }.ScheduleParallel();
    }

    [BurstCompile]
    partial struct SyncStaticJob : IJobEntity
    {
        void Execute(
                in VATAsset asset,
                in VATAnimationState animState,
                ref VATClipStartFrame clipStart,
                ref VATClipFrameCount clipFrames,
                ref VATClipStartTime clipStartTime,
                ref VATPrevClipStartFrame prevClipStart,
                ref VATPrevClipFrameCount prevClipFrames,
                ref VATPrevClipStartTime prevClipStartTime,
                ref VATBlendFactor blendFactor)
        {
            ref var blob = ref asset.Blob.Value;
            ref var clip = ref blob.Clips[animState.CurrentClipIndex];

            clipStart.Value = clip.StartFrame;
            clipFrames.Value = clip.FrameCount;
            clipStartTime.Value = animState.CurrentClipStartTime;

            prevClipStart.Value = clip.StartFrame;
            prevClipFrames.Value = clip.FrameCount;
            prevClipStartTime.Value = animState.CurrentClipStartTime;

            blendFactor.Value = 1f;
        }
    }

    [BurstCompile]
    partial struct SyncTransitionJob : IJobEntity
    {
        public float Now;
        void Execute(
                in VATAsset asset,
                in VATAnimationState animState,
                in VATAnimationTransition trans,
                ref VATClipStartFrame clipStart,
                ref VATClipFrameCount clipFrames,
                ref VATClipStartTime clipStartTime,
                ref VATPrevClipStartFrame prevClipStart,
                ref VATPrevClipFrameCount prevClipFrames,
                ref VATPrevClipStartTime prevClipStartTime,
                ref VATBlendFactor blendFactor)
        {
            ref var blob = ref asset.Blob.Value;
            ref var curr = ref blob.Clips[animState.CurrentClipIndex];
            ref var prev = ref blob.Clips[trans.PrevClipIndex];

            clipStart.Value = curr.StartFrame;
            clipFrames.Value = curr.FrameCount;
            clipStartTime.Value = animState.CurrentClipStartTime;

            prevClipStart.Value = prev.StartFrame;
            prevClipFrames.Value = prev.FrameCount;
            prevClipStartTime.Value = trans.PrevClipStartTime;

            float elapsed = Now - trans.TransitionStartTime;
            float t = math.saturate(elapsed / trans.TransitionDuration);
            blendFactor.Value = t;
        }
    }
}