using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct PlayerDamageQueueSingleton : IComponentData
{
    public NativeQueue<float> queue;
}
