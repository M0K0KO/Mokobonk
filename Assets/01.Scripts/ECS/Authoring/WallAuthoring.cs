using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public class WallAuthoring : MonoBehaviour
{
    [Header("Collider")]
    public float Size = 1f;

    private class Baker : Baker<WallAuthoring>
    {
        public override void Bake(WallAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<WallTag>(entity);


            var collider = Unity.Physics.BoxCollider.Create(
                new Unity.Physics.BoxGeometry
                {
                    Center = new Unity.Mathematics.float3(0, authoring.Size * 0.5f, 0f),
                    Size = new Unity.Mathematics.float3(authoring.Size, authoring.Size, authoring.Size),
                    Orientation = Quaternion.identity,
                    BevelRadius = 0f
                },
                new Unity.Physics.CollisionFilter
                {
                    BelongsTo = CollisionLayers.Wall,
                    CollidesWith = CollisionLayers.Enemy,
                    GroupIndex = 0
                },
                new Unity.Physics.Material
                {
                    CollisionResponse = CollisionResponsePolicy.Collide,
                }
            );
            AddComponent(entity, new PhysicsCollider { Value = collider });
            AddComponent(entity, new PhysicsVelocity());
            AddComponent(entity, PhysicsMass.CreateKinematic(MassProperties.UnitSphere));
            AddSharedComponent(entity, new PhysicsWorldIndex { Value = 0 });

        }
    }
}