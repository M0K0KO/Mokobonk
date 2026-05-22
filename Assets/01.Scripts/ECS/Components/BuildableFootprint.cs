using Unity.Entities;
using Unity.Mathematics;

public struct BuildableFootprint : IComponentData
{
    public int2 Anchor;
    public int2 Size;
}