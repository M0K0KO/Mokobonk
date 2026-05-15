using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerSyncBridge : MonoBehaviour
{
    private EntityManager _entityManager;
    private Entity _playerEntity;

    private void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        EntityQuery query = _entityManager.CreateEntityQuery(
            typeof(PlayerPositionSingleton)
            );

        _playerEntity = query.GetSingletonEntity();
    }

    private void Update()
    {
        float3 pos = transform.position;

        _entityManager.SetComponentData(_playerEntity, new PlayerPositionSingleton { Position = pos });
    }
}
