using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct SpawnTurretCommand
{
    public int2 Cell;
}

public struct SpawnTurretQueueSingleton : IComponentData
{
    public NativeQueue<SpawnTurretCommand> Queue;
}