using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class PlayerSyncBridge : MonoBehaviour
{
    public float colliderRadius = 0.5f;
    public float maxHP = 100f;

    private World _world;
    private EntityManager _entityManager;
    private Entity _playerEntity;
    private float _currentHP;

    private void Start()
    {
        _world = World.DefaultGameObjectInjectionWorld;
        _entityManager = _world.EntityManager;
        _currentHP = maxHP;

        if (!_entityManager.CreateEntityQuery(typeof(PlayerPositionSingleton)).HasSingleton<PlayerPositionSingleton>())
            _entityManager.CreateSingleton<PlayerPositionSingleton>();

        _playerEntity = _entityManager.CreateEntity();
        _entityManager.AddComponent<PlayerTag>(_playerEntity);
        _entityManager.AddComponentData(_playerEntity, LocalTransform.FromPosition(transform.position));

        _entityManager.AddSharedComponent(_playerEntity, new PhysicsWorldIndex { Value = 0 });
        var collider = Unity.Physics.CapsuleCollider.Create(
            new CapsuleGeometry { Radius = colliderRadius, Vertex0 = new float3(0.0f, -0.5f, 0.0f), Vertex1 = new float3(0.0f, 0.5f, 0.0f) },
            new CollisionFilter
            {
                BelongsTo = CollisionLayers.Player,
                CollidesWith = CollisionLayers.Enemy,
                GroupIndex = 0
            },
            new Unity.Physics.Material { CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents }
        );
        _entityManager.AddComponentData(_playerEntity, new PhysicsCollider { Value = collider });
        _entityManager.AddComponentData(_playerEntity, PhysicsVelocity.Zero);
        _entityManager.AddComponentData(_playerEntity, PhysicsMass.CreateKinematic(MassProperties.UnitSphere));

        var queue = new NativeQueue<float>(Allocator.Persistent);
        _entityManager.CreateSingleton(new PlayerDamageQueueSingleton { queue = queue });
    }

    private void Update()
    {
        float3 pos = transform.position;
        _entityManager.SetComponentData(_playerEntity, LocalTransform.FromPosition(pos));

        var posSingleton = _entityManager.CreateEntityQuery(typeof(PlayerPositionSingleton))
                     .GetSingletonRW<PlayerPositionSingleton>();
        posSingleton.ValueRW.Position = pos;

        _entityManager.CompleteAllTrackedJobs();

        var dmgQueue = _entityManager.CreateEntityQuery(typeof(PlayerDamageQueueSingleton)).GetSingleton<PlayerDamageQueueSingleton>().queue;

        while (dmgQueue.TryDequeue(out var dmg))
        {
            _currentHP -= dmg;
            Debug.Log($"Player HP : {_currentHP}/{maxHP} (damage {dmg})");

            if (_currentHP <= 0f)
            {
                Debug.Log("=== GAME OVER ===");
                enabled = false;
                Time.timeScale = 0f;
                return;
            }
        }
    }
    private void OnDestroy()
    {
        if (_world == null || !_world.IsCreated)
            return;

        _entityManager.CompleteAllTrackedJobs();

        if (_playerEntity != Entity.Null && _entityManager.Exists(_playerEntity))
        {
            _entityManager.DestroyEntity(_playerEntity);
            _playerEntity = Entity.Null;
        }

        var query = _entityManager.CreateEntityQuery(typeof(PlayerDamageQueueSingleton));

        if (query.HasSingleton<PlayerDamageQueueSingleton>())
        {
            var dmgQueueSingleton = query.GetSingleton<PlayerDamageQueueSingleton>();

            if (dmgQueueSingleton.queue.IsCreated)
                dmgQueueSingleton.queue.Dispose();

            _entityManager.DestroyEntity(query);
        }
    }
}
