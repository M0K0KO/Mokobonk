using Unity.Entities;
using UnityEngine;

public class ResourceNodeAuthoring : MonoBehaviour
{
    public int YieldPerTick = 5;
    public float TickInterval = 1f;

    class Baker : Baker<ResourceNodeAuthoring>
    {
        public override void Bake(ResourceNodeAuthoring src)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ResourceNode
            {
                YieldPerTick = src.YieldPerTick,
                TickInterval = src.TickInterval,
            });
            AddComponent(entity, new ResourceNodeState
            {
                NextTickTime = 0f,
            });
            AddComponent<ResourceNodeTag>(entity);
        }
    }
}