using Unity.Entities;
using UnityEngine;

public class WallSpawnConfigAuthoring: MonoBehaviour
{
    public GameObject WallPrefab;
    public int Cost = 10;

    private class Baker : Baker<WallSpawnConfigAuthoring>
    {
        public override void Bake(WallSpawnConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new WallSpawnConfigSingleton
            {
                WallPrefab = GetEntity(authoring.WallPrefab, TransformUsageFlags.Dynamic),
                Cost = authoring.Cost
            });
        }
    }
}