using Unity.Collections;
using Unity.Entities;

public struct CoreDamageQueueSingleton : IComponentData
{
    public NativeQueue<float> Queue;
}