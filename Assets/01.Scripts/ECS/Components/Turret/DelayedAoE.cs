using Unity.Entities;
using Unity.Mathematics;

public struct DelayedAoE : IComponentData
{
    public float3 Position;
    public float Radius;
    public float Damage;
    public float ExplodeTime;
}