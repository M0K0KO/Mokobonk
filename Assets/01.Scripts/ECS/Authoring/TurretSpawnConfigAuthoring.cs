using Unity.Entities;
using UnityEngine;

public class TurretSpawnConfigAuthoring : MonoBehaviour
{
    public GameObject TurretPrefab;
    public int TurretCost;
    private class Baker : Baker<TurretSpawnConfigAuthoring>
    {
        public override void Bake(TurretSpawnConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new TurretSpawnConfigSingleton
            {
                TurretPrefab = GetEntity(authoring.TurretPrefab, TransformUsageFlags.Dynamic),
                Cost = authoring.TurretCost
            });
        }
    }
}
