using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public class CoreAuthoring : MonoBehaviour
{
    [Header("Core Stats")]
    public float MaxHealth = 1000f;

    private class Baker : Baker<CoreAuthoring>
    {
        public override void Bake(CoreAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<CoreTag>(entity);
            AddComponent(entity, new Health
            {
                Current = authoring.MaxHealth,
                Max = authoring.MaxHealth
            });

            AddComponent(entity, new CorePositionSingleton
            {
                Value = authoring.transform.position,
            });

            var collider = Unity.Physics.CylinderCollider.Create(
                new Unity.Physics.CylinderGeometry { Center = new float3(0, 0.5f, 0), Radius = 1f, Height = 0.5f, Orientation = quaternion.RotateX(math.radians(90f)), SideCount = 16 },
                new Unity.Physics.CollisionFilter
                {
                    BelongsTo = CollisionLayers.Core,
                    CollidesWith = CollisionLayers.Enemy,
                    GroupIndex = 0
                },
                new Unity.Physics.Material
                {
                    CollisionResponse = Unity.Physics.CollisionResponsePolicy.RaiseTriggerEvents
                }
            );
            AddComponent(entity, new PhysicsCollider { Value = collider });
            AddSharedComponent(entity, new PhysicsWorldIndex { Value = 0 });
        }
    }
}

