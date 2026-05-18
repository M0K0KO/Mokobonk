using Unity.Entities;

public static class VATAnimationCommands
{
    public static void PlayClip(
        EntityCommandBuffer.ParallelWriter ecb,
        int sortKey,
        Entity entity,
        in VATAnimationState currentState,
        int newClipIndex,
        float blendDuration,
        float currentTime)
    {
        if (currentState.CurrentClipIndex == newClipIndex) return;

        if (blendDuration > 0f)
        {
            ecb.AddComponent(sortKey, entity, new VATAnimationTransition
            {
                PrevClipIndex = currentState.CurrentClipIndex,
                PrevClipStartTime = currentState.CurrentClipStartTime,
                TransitionStartTime = currentTime,
                TransitionDuration = blendDuration
            });
        }

        ecb.SetComponent(sortKey, entity, new VATAnimationState
        {
            CurrentClipIndex = newClipIndex,
            CurrentClipStartTime = currentTime
        });
    }


    public static void PlayClip(
            EntityCommandBuffer ecb,
            Entity entity,
            in VATAnimationState currentState,
            int newClipIndex,
            float blendDuration,
            float currentTime)
    {
        if (currentState.CurrentClipIndex == newClipIndex) return;

        if (blendDuration > 0f)
        {
            ecb.AddComponent(entity, new VATAnimationTransition
            {
                PrevClipIndex = currentState.CurrentClipIndex,
                PrevClipStartTime = currentState.CurrentClipStartTime,
                TransitionStartTime = currentTime,
                TransitionDuration = blendDuration
            });
        }

        ecb.SetComponent(entity, new VATAnimationState
        {
            CurrentClipIndex = newClipIndex,
            CurrentClipStartTime = currentTime
        });
    }
}