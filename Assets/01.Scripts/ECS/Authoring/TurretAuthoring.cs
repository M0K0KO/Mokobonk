using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEditor.PackageManager;
using UnityEngine;

public class TurretAuthoring : MonoBehaviour
{
    [Header("Stats")]
    public float Range = 8f;
    public float FireRate = 2f;
    public float Damage = 10f;
    public float ProjectileSpeed = 20f;

    [Header("Projectile")]
    public GameObject ProjectilePrefab;

    private class Baker : Baker<TurretAuthoring>
    {
        public override void Bake(TurretAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<TurretTag>(entity);
            AddComponent(entity, new TurretStats
            {
                Range = authoring.Range,
                FireRate = authoring.FireRate,
                Cooldown = 0f,
                Damage = authoring.Damage,
                ProjectileSpeed = authoring.ProjectileSpeed,
                ProjectilePrefab = GetEntity(authoring.ProjectilePrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}