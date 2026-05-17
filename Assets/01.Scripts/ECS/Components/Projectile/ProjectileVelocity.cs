using Unity.Entities;
using Unity.Mathematics;

public struct ProjectileVelocity : IComponentData
{
    public float Speed;
    public float3 Direction;
}
