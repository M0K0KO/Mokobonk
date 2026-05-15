using Unity.Entities;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval;
    [SerializeField] private float spawnRadiusMin;
    [SerializeField] private float spawnRadiusMax;
    [SerializeField] private int maxEnemies;

    class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            Entity prefabEntity = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic);

            AddComponent(entity, new GameConfig 
            { 
                enemyPrefab = prefabEntity,
                spawnInterval = authoring.spawnInterval,
                spawnRadiusMin = authoring.spawnRadiusMin,
                spawnRadiusMax = authoring.spawnRadiusMax,
                maxEnemies = authoring.maxEnemies,
            });
        }
    }
}
