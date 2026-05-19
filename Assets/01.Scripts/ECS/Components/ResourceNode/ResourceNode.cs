
using Unity.Entities;

public struct ResourceNode : IComponentData
{
    public int YieldPerTick;
    public float TickInterval;
}