using Unity.Entities;

public struct BuildableHealth : IComponentData
{
    public float Current;
    public float Max;
}