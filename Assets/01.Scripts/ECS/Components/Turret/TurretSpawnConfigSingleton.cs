using Unity.Entities;

public struct TurretSpawnConfigSingleton : IComponentData
{
    public Entity TurretPrefab;
    public int Cost;
}