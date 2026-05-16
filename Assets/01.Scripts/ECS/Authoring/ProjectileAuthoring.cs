using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public class ProjectileAuthoring : MonoBehaviour
{
    public float damage = 10f;

    class Baker : Baker<ProjectileAuthoring>
    {
        public override void Bake(ProjectileAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new ProjectileTag());
            AddComponent(entity, new ProjectileDamage { Value = authoring.damage });
            AddComponent(entity, new ProjectileDirection { Value = new float3(0, 0, 1) });
            AddComponent(entity, new Lifetime { Remaining = 0f });

            var collider = Unity.Physics.SphereCollider.Create(
                new Unity.Physics.SphereGeometry { Center = float3.zero, Radius = 0.2f },
                new CollisionFilter
                {
                    BelongsTo = CollisionLayers.Projectile,
                    CollidesWith = CollisionLayers.Enemy,
                    GroupIndex = 0
                },
                new Unity.Physics.Material
                {
                    CollisionResponse = Unity.Physics.CollisionResponsePolicy.RaiseTriggerEvents,
                }
            );

            AddComponent(entity, new PhysicsCollider { Value = collider });
        }
    }
}