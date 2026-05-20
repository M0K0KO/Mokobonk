using Unity.Entities;
using Unity.Mathematics;

public struct ExplosionFiredEvent : IComponentData
{
    public float3 Position;
    public float Radius;
    public VfxId VfxId;
    public float SpawnTime;
}