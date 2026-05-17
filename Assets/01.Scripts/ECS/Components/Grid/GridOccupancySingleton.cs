using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct GridOccupancySingleton : IComponentData
{
    public NativeHashMap<int2, Entity> Map;
}