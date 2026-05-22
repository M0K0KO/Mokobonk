using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BuildableRegistryAuthoring : MonoBehaviour
{
    [System.Serializable]
    public struct Entry
    {
        public BuildableKind Kind;
        public GameObject Prefab;
        public int Cost;
        public bool BlocksMovement;
        public float MaxHealth;
        public int2 Size;
    }

    public Entry[] Entries;

    class Baker : Baker<BuildableRegistryAuthoring>
    {
        public override void Bake(BuildableRegistryAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            var buffer = AddBuffer<BuildableRegistryEntryBuffer>(entity);
            foreach (var e in authoring.Entries)
            {
                buffer.Add(new BuildableRegistryEntryBuffer
                {
                    Kind = e.Kind,
                    Prefab = GetEntity(e.Prefab, TransformUsageFlags.Dynamic),
                    Cost = e.Cost,
                    BlocksMovement = e.BlocksMovement,
                    MaxHealth = e.MaxHealth,
                    Size = e.Size
                });
            }
        }
    }
}

public struct BuildableRegistryEntryBuffer : IBufferElementData
{
    public BuildableKind Kind;
    public Entity Prefab;
    public int Cost;
    public bool BlocksMovement;
    public float MaxHealth;
    public int2 Size;
}