using Unity.Entities;
using UnityEngine;

public class EnemySpawnConfigAuthoring : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public float SpawnInterval = 0.5f;

    private class Baker : Baker<EnemySpawnConfigAuthoring>
    {
        public override void Bake(EnemySpawnConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new EnemySpawnConfigSingleton
            {
                EnemyPrefab = GetEntity(authoring.EnemyPrefab, TransformUsageFlags.Dynamic),
                SpawnInterval = authoring.SpawnInterval,
                NextSpawnTime = 0f
            });
        }
    }
}