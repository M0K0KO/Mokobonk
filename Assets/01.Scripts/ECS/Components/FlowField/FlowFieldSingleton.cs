using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct FlowFieldSingleton : IComponentData
{
    // index = x + y * gridWidth, value = normalized Direction
    public NativeArray<float3> Directions;
    public NativeArray<ushort> Costs;
    public int Width;
    public int Height;
}