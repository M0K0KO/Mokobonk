using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public class ProjectileAuthoring : MonoBehaviour
{
    [Header("Stats")]
    public float Lifetime = 3;

    [Header("Collider")]
    public float Radius = 0.15f;

    private class Baker : Baker<ProjectileAuthoring>
    {
        public override void Bake(ProjectileAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<ProjectileTag>(entity);
            AddComponent(entity, new ProjectileDamage { Value = 0f });
            AddComponent(entity, new ProjectileVelocity
            {
                Direction = new float3(0, 0, 1),
                Speed = 0f
            });
            AddComponent(entity, new Lifetime { Remaining = authoring.Lifetime });

            var collider = Unity.Physics.SphereCollider.Create(
                new SphereGeometry { Center = float3.zero, Radius = authoring.Radius },
                new CollisionFilter
                {
                    BelongsTo = CollisionLayers.Projectile,
                    CollidesWith = CollisionLayers.Enemy,
                    GroupIndex = 0
                },
                new Unity.Physics.Material
                {
                    CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents
                }
            );
            AddComponent(entity, new PhysicsCollider { Value = collider });

            AddComponent(entity, new PhysicsVelocity());
            AddComponent(entity, PhysicsMass.CreateKinematic(MassProperties.UnitSphere));
            AddSharedComponent(entity, new PhysicsWorldIndex { Value = 0 });
        }
    }
}