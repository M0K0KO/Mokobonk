using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct BuildCommand
{
    public int2 Cell;
    public BuildableKind Kind;
}

public struct SpawnBuildQueueSingleton : IComponentData
{
    public NativeQueue<BuildCommand> Queue;
}