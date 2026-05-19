using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct BuildableRegistryInitSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BuildableRegistryEntryBuffer>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.HasSingleton<BuildableRegistrySingleton>())
        {
            state.Enabled = false;
            return;
        }

        var entries = new NativeList<BuildableRegistryEntryBuffer>(8, Allocator.Temp);

        foreach (var buffer in SystemAPI.Query<DynamicBuffer<BuildableRegistryEntryBuffer>>())
        {
            for (int i = 0; i < buffer.Length; i++)
                entries.Add(buffer[i]);
            break;
        }

        var map = new NativeHashMap<byte, BuildableInfo>(entries.Length, Allocator.Persistent);
        for (int i = 0; i < entries.Length; i++)
        {
            var e = entries[i];
            map.Add((byte)e.Kind, new BuildableInfo
            {
                Kind = e.Kind,
                Prefab = e.Prefab,
                Cost = e.Cost,
                BlocksMovement = e.BlocksMovement,
            });
        }
        entries.Dispose();

        var singletonEntity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(singletonEntity,
            new BuildableRegistrySingleton { Map = map });

        state.Enabled = false;
    }

    public void OnDestroy(ref SystemState state)
    {
        if (SystemAPI.HasSingleton<BuildableRegistrySingleton>())
        {
            var reg = SystemAPI.GetSingleton<BuildableRegistrySingleton>();
            if (reg.Map.IsCreated) reg.Map.Dispose();
        }
    }
}