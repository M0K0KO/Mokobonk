using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct SpawnWallCommand
{
    public int2 Cell;
}

public struct SpawnWallQueueSingleton : IComponentData
{
    public NativeQueue<SpawnWallCommand> Queue;
}