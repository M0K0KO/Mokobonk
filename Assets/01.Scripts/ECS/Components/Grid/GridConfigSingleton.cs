using Unity.Entities;
using Unity.Mathematics;

public struct GridConfigSingleton : IComponentData
{
    public float CellSize;
    public int2 GridSize;
    public float3 Origin;
}
