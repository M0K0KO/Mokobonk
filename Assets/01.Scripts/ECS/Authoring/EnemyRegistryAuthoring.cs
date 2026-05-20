using Unity.Entities;
using UnityEngine;

public class EnemyRegistryAuthoring : MonoBehaviour
{
    [System.Serializable]
    public struct Entry
    {
        public EnemyKind Kind;
        public GameObject Prefab;
        public float SpawnWeight;
    }

    public Entry[] Entries;

    class Baker : Baker<EnemyRegistryAuthoring>
    {
        public override void Bake(EnemyRegistryAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var buffer = AddBuffer<EnemyRegistryEntryBuffer>(entity);

            foreach (var e in authoring.Entries)
            {
                buffer.Add(new EnemyRegistryEntryBuffer
                {
                    Kind = e.Kind,
                    Prefab = GetEntity(e.Prefab, TransformUsageFlags.Dynamic),
                    SpawnWeight = e.SpawnWeight,
                });
            }
        }
    }
}