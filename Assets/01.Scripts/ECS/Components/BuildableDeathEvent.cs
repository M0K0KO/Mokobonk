using Unity.Entities;
using Unity.Mathematics;

public struct BuildableDeathEvent : IComponentData
{
    public float3 Position;
    public float SpawnTime;
    public VfxId VfxId;
}