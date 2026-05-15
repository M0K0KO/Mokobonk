using Unity.Entities;

public struct GameConfig : IComponentData
{
    public Entity enemyPrefab;
    public float spawnInterval;
    public float spawnRadiusMin;
    public float spawnRadiusMax;
    public int maxEnemies;
}
