using Unity.Entities;
using UnityEngine;

public class WeaponConfigAuthoring : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float fireRate = 0.3f;
    public float projectileSpeed = 20f;
    public float projectileLifetime = 2f;
    public float projectileDamage = 10f;

    class Baker : Baker<WeaponConfigAuthoring>
    {
        public override void Bake(WeaponConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var prefabEntity = GetEntity(authoring.projectilePrefab, TransformUsageFlags.Dynamic);

            AddComponent(entity, new WeaponState
            {
                cooldownRemaining = 0f,
                fireRate = authoring.fireRate,
                projectileSpeed = authoring.projectileSpeed,
                projectileLifetime = authoring.projectileLifetime,
                projectileDamage = authoring.projectileDamage,
                projectilePrefab = prefabEntity
            });
        }
    }
}
