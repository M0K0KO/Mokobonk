using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    [Header("Enemy Properties")]
    [SerializeField] private float maxHealth;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float contactDamage;

    class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new EnemyTag { });
            AddComponent(entity, new Health { Max = authoring.maxHealth, Current = authoring.maxHealth});
            AddComponent(entity, new MoveSpeed { Speed = authoring.moveSpeed });
            AddComponent(entity, new RotateSpeed { Speed = authoring.rotateSpeed });

            var collider = Unity.Physics.SphereCollider.Create(
                new Unity.Physics.SphereGeometry { Center = float3.zero, Radius = 0.55f },
                new Unity.Physics.CollisionFilter
                {
                    BelongsTo = CollisionLayers.Enemy,
                    CollidesWith = CollisionLayers.Enemy | CollisionLayers.Projectile | CollisionLayers.Player,
                    GroupIndex = 0
                }
            );
            AddComponent(entity, new PhysicsCollider { Value = collider });
            AddComponent(entity, new ContactDamage { Value = authoring.contactDamage });
        }
    }
}
