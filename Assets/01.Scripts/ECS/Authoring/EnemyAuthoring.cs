using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    [Header("Enemy Properties")]
    [SerializeField] private EnemyKind Kind = EnemyKind.Runner;
    [SerializeField] private float maxHealth;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float contactDamage;

    [Header("Attack")]
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float attackInterval = 1.0f;
    [SerializeField] private float attackRange = 1.2f;

    [Header("Collider")]
    [SerializeField] private float capsuleRadius = 0.25f;
    [SerializeField] private float capsuleHeight = 1.0f;

    class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new EnemyTag { });
            AddComponent(entity, new EnemyKindComponent { Value = authoring.Kind });
            AddComponent(entity, new Health { Max = authoring.maxHealth, Current = authoring.maxHealth});
            AddComponent(entity, new EnemyAttackStats
            {
                Damage = authoring.attackDamage,
                Interval = authoring.attackInterval,
                Range = authoring.attackRange,
                Cooldown = 0f,
            });
            AddComponent(entity, new MoveSpeed { Speed = authoring.moveSpeed });
            AddComponent(entity, new RotateSpeed { Speed = authoring.rotateSpeed });

            float radius = authoring.capsuleRadius;
            float height = authoring.capsuleHeight;
            float segmentHalf = math.max(0, (height - 2 * radius) * 0.5f);

            var collider = Unity.Physics.CapsuleCollider.Create(
                new Unity.Physics.CapsuleGeometry
                {
                    Vertex0 = new float3(0, radius, 0),
                    Vertex1 = new float3(0, radius + segmentHalf * 2, 0),
                    Radius = radius
                },
                new Unity.Physics.CollisionFilter
                {
                    BelongsTo = CollisionLayers.Enemy,
                    CollidesWith = CollisionLayers.Projectile | CollisionLayers.Core | CollisionLayers.Wall | CollisionLayers.Turret,
                    GroupIndex = 0
                },
                new Unity.Physics.Material
                {
                    CollisionResponse = CollisionResponsePolicy.CollideRaiseCollisionEvents
                }
            );
            AddBlobAsset(ref collider, out _);
            AddComponent(entity, new PhysicsCollider { Value = collider });
            AddSharedComponent(entity, new PhysicsWorldIndex { Value = 0 });

            var mass = PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 1f);
            mass.InverseInertia = new float3(0f, mass.InverseInertia.y, 0f);
            AddComponent(entity, mass);
            AddComponent(entity, new PhysicsVelocity());
            AddComponent(entity, new ContactDamage { Value = authoring.contactDamage });

            AddComponent<AttackingTag>(entity);
            SetComponentEnabled<AttackingTag>(entity, false);

            AddComponent(entity, new EnemyAttackTarget { Value = Entity.Null });
        }
    }
}
