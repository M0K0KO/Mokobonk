using Unity.Entities;

public struct WallSpawnConfigSingleton : IComponentData
{
    public Entity WallPrefab;
    public int Cost;
}