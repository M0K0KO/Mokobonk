using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public sealed class VfxBridge : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private VfxRegistry _registry;
    [SerializeField] private int _maxEventsPerFrame = 50;
    [SerializeField] private float _maxEventAge = 0.1f;
    [SerializeField] private int _maxQueueSize = 500;

    private VfxPool _pool;
    private EntityManager _entityManager;
    private EntityQuery _eventQuery;
    private readonly Queue<HitscanFiredEvent> _queue = new(512);

    private void Start()
    {
        _registry.BuildLookup();

        _pool = new VfxPool(transform);
        foreach (var e in _registry.Entries)
            _pool.Register(e.Id, e.Prefab, e.PrewarmCount);

        var world = World.DefaultGameObjectInjectionWorld;
        _entityManager = world.EntityManager;
        _eventQuery = _entityManager.CreateEntityQuery(typeof(HitscanFiredEvent));
    }

    private void Update()
    {
        CollectEvents();
        DispatchEvents();
    }

    private void CollectEvents()
    {
        if (_eventQuery.IsEmpty) return;

        var events = _eventQuery.ToComponentDataArray<HitscanFiredEvent>(Unity.Collections.Allocator.Temp);
        var entities = _eventQuery.ToEntityArray(Unity.Collections.Allocator.Temp);

        for (int i = 0; i < events.Length; i++)
        {
            if (_queue.Count >= _maxQueueSize) break;
            _queue.Enqueue(events[i]);
        }

        _entityManager.DestroyEntity(_eventQuery);

        events.Dispose();
        entities.Dispose();
    }

    private void DispatchEvents()
    {
        float now = (float)World.DefaultGameObjectInjectionWorld.Time.ElapsedTime;

        int processed = 0;
        
        while (_queue.Count > 0 && processed < _maxEventsPerFrame)
        {
            var evt = _queue.Dequeue();

            if (now - evt.SpawnTime > _maxEventAge) continue;

            Play(evt);
            Play(evt);
            processed++;
        }
    }
    private void Play(in HitscanFiredEvent evt)
    {
        if (evt.MuzzleId != VfxId.None)
        {
            var inst = _pool.Rent(evt.MuzzleId);
            inst.Play(new VfxPlayParams
            {
                Origin = evt.Origin,
                Lifetime = 0.15f,
            });
        }

        if (evt.BeamId != VfxId.None)
        {
            var inst = _pool.Rent(evt.BeamId);
            inst.Play(new VfxPlayParams
            {
                Origin = evt.Origin,
                End = evt.Hit,
                Lifetime = 0.08f,
            });
        }

        if (evt.HitSparkId != VfxId.None)
        {
            var inst = _pool.Rent(evt.HitSparkId);
            inst.Play(new VfxPlayParams
            {
                Origin = evt.Hit,
                Lifetime = 0.25f,
            });
        }
    }
}