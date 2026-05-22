using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public enum BuildableKind : byte
{
    None = 0,
    Turret_Gunner = 1,
    Turret_Mortar = 2,
    Wall = 3,
}

public struct BuildableInfo
{
    public BuildableKind Kind;
    public Entity Prefab;
    public int Cost;
    public bool BlocksMovement;
    public float MaxHealth;
    public int2 Size;
}

public struct BuildableRegistrySingleton : IComponentData
{
    public NativeHashMap<byte, BuildableInfo> Map;

    public bool TryGet(BuildableKind kind, out BuildableInfo info)
    {
        return Map.TryGetValue((byte)kind, out info);
    }
}