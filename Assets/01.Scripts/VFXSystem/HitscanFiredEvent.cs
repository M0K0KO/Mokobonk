using Unity.Entities;
using Unity.Mathematics;

public struct HitscanFiredEvent : IComponentData
{
    public float3 Origin;
    public float3 Hit;
    public VfxId MuzzleId;
    public VfxId BeamId;
    public VfxId HitSparkId;
    public float SpawnTime;
}