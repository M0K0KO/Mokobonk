using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct CoreInfluenceGridSingleton : IComponentData
{
    public NativeHashSet<int2> Cells;
}