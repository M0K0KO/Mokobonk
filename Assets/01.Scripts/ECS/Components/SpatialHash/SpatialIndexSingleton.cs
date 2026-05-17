using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct EnemySpatialEntry
{
    public Entity Entity;
    public float3 Position;
}

public struct SpatialIndexSingleton : IComponentData
{
    public NativeParallelMultiHashMap<int2, EnemySpatialEntry> Map;
    public float CellSize;
    public float3 Origin;
}