using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public class MortarAuthoring : MonoBehaviour
{
    public float Range = 18f;
    public float FireRate = 1f;
    public float Damage = 40f;
    public float AoERadius = 3f;
    public float ExplodeDelay = 1f;

    class Baker : Baker<MortarAuthoring>
    {
        public override void Bake(MortarAuthoring src)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildableFootprint { Anchor = int2.zero });
            AddComponent(entity, new MortarStats
            {
                Range = src.Range,
                FireRate = src.FireRate,
                Damage = src.Damage,
                AoERadius = src.AoERadius,
                ExplodeDelay = src.ExplodeDelay,
                Cooldown = 0f,
            });
            AddComponent<MortarTag>(entity);

            var collider = Unity.Physics.BoxCollider.Create(
                new Unity.Physics.BoxGeometry
                {
                    Center = new Unity.Mathematics.float3(0, 0.5f, 0f),
                    Size = new Unity.Mathematics.float3(2f, 1f, 2f),
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