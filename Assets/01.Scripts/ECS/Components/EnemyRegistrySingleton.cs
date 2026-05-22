using Unity.Collections;
using Unity.Entities;

public struct EnemyInfo
{
    public EnemyKind Kind;
    public Entity Prefab;
    public float SpawnWeight;

    public float Damage;
    public float Interval;
    public float Range;
}

public struct EnemyRegistryEntryBuffer : IBufferElementData
{
    public EnemyKind Kind;
    public Entity Prefab;
    public float SpawnWeight;

    public float Damage;
    public float Interval;
    public float Range;
}

public struct EnemyRegistrySingleton : IComponentData
{
    public NativeHashMap<byte, EnemyInfo> Map;
    public float TotalWeight;

    public bool TryGet(EnemyKind kind, out EnemyInfo info)
    {
        return Map.TryGetValue((byte)kind, out info);
    }
}