using Unity.Entities;
using UnityEngine;

public class EnemySpawnConfigAuthoring : MonoBehaviour
{
    public float SpawnInterval = 0.5f;

    private class Baker : Baker<EnemySpawnConfigAuthoring>
    {
        public override void Bake(EnemySpawnConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new EnemySpawnConfigSingleton
            {
                SpawnInterval = authoring.SpawnInterval,
                NextSpawnTime = 0f
            });
        }
    }
}