using Unity.Entities;

public struct EnemySpawnConfigSingleton : IComponentData
{
    public float SpawnInterval;
    public float NextSpawnTime;
    public int RemainingToSpawn;
}