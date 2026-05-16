using Unity.Entities;

public struct EnemySpawnConfigSingleton : IComponentData
{
    public Entity EnemyPrefab;
    public float SpawnInterval;
    public float NextSpawnTime;
}