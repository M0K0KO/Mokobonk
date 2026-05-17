using Unity.Entities;
using UnityEngine;

public class ResourceConfigAuthoring : MonoBehaviour
{
    public int InitialGold = 200;
    public int EnemyKillReward = 5;

    private class Baker : Baker<ResourceConfigAuthoring>
    {
        public override void Bake(ResourceConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ResourceSingleton
            {
                Gold = authoring.InitialGold,
                EnemyKillReward = authoring.EnemyKillReward,
            });
        }
    }
}