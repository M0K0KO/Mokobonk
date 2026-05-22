using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct EnemyRegistryInitSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyRegistryEntryBuffer>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.HasSingleton<EnemyRegistrySingleton>())
        {
            state.Enabled = false;
            return;
        }

        var entries = new NativeList<EnemyRegistryEntryBuffer>(8, Allocator.Temp);
        foreach (var buffer in SystemAPI.Query<DynamicBuffer<EnemyRegistryEntryBuffer>>())
        {
            for (int i = 0; i < buffer.Length; i++)
                entries.Add(buffer[i]);
            break;
        }

        var map = new NativeHashMap<byte, EnemyInfo>(entries.Length, Allocator.Persistent);
        float total = 0f;
        for (int i = 0; i < entries.Length; i++)
        {
            var e = entries[i];
            map.Add((byte)e.Kind, new EnemyInfo
            {
                Kind = e.Kind,
                Prefab = e.Prefab,
                SpawnWeight = e.SpawnWeight,

                Damage = e.Damage,
                Interval = e.Interval,
                Range = e.Range,
            });
            total += e.SpawnWeight;
        }
        entries.Dispose();

        var singletonEntity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(singletonEntity,
            new EnemyRegistrySingleton { Map = map, TotalWeight = total });

        state.Enabled = false;
    }

    public void OnDestroy(ref SystemState state)
    {
        if (SystemAPI.HasSingleton<EnemyRegistrySingleton>())
        {
            var reg = SystemAPI.GetSingleton<EnemyRegistrySingleton>();
            if (reg.Map.IsCreated) reg.Map.Dispose();
        }
    }
}