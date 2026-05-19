using Unity.Entities;
using Unity.Mathematics;

public struct EnemyKilledEvent : IComponentData
{
    public float3 Position;
    public int EnemyType;
}