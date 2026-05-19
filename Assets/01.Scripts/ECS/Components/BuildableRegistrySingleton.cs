using Unity.Collections;
using Unity.Entities;

public enum BuildableKind : byte
{
    None = 0,
    Turret_Gunner = 1,
    Wall = 2,
}

public struct BuildableInfo
{
    public BuildableKind Kind;
    public Entity Prefab;
    public int Cost;
    public bool BlocksMovement;
}

public struct BuildableRegistrySingleton : IComponentData
{
    public NativeHashMap<byte, BuildableInfo> Map;

    public bool TryGet(BuildableKind kind, out BuildableInfo info)
    {
        return Map.TryGetValue((byte)kind, out info);
    }
}