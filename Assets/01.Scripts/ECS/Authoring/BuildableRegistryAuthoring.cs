using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
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
}