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

            var collider = Unity.Physics.BoxCollider.Create(
                new Unity.Physics.BoxGeometry
                {
                    Center = new Unity.Mathematics.float3(0, 0.5f, 0f),
                    Size = new Unity.Mathematics.float3(1f, 1f, 1f),
                    Orientation = Quaternion.identity,
                    BevelRadius = 0f
                },
                new Unity.Physics.CollisionFilter
                {
                    BelongsTo = CollisionLayers.Turret,
                    CollidesWith = CollisionLayers.Enemy,
                    GroupIndex = 0
                },
                new Unity.Physics.Material
                {
                    CollisionResponse = CollisionResponsePolicy.Collide,
                }
            );
            AddBlobAsset(ref collider, out _);
            AddComponent(entity, new PhysicsCollider { Value = collider });
            AddComponent(entity, new PhysicsVelocity());
            AddComponent(entity, PhysicsMass.CreateKinematic(MassProperties.UnitSphere));
            AddSharedComponent(entity, new PhysicsWorldIndex { Value = 0 });
        }
    }
}