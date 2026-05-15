using Unity.Entities;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    [Header("Enemy Properties")]
    [SerializeField] private float maxHealth;
    [SerializeField] private float moveSpeed;

    class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new EnemyTag { });
            AddComponent(entity, new Health { Max = authoring.maxHealth, Current = authoring.maxHealth});
            AddComponent(entity, new MoveSpeed { Speed = authoring.moveSpeed });
        }
    }
}
