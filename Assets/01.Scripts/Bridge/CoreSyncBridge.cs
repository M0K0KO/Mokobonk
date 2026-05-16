using TMPro;
using Unity.Entities;
using UnityEngine;

public class CoreSyncBridge : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;

    private EntityManager _entityManager;
    private EntityQuery _coreQuery;
    private EntityQuery _stateQuery;

    private void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _coreQuery = _entityManager.CreateEntityQuery(typeof(Health), typeof(CoreTag));
        _stateQuery = _entityManager.CreateEntityQuery(typeof(GameStateSingleton));
    }

    private void Update()
    {
        if (!_coreQuery.TryGetSingleton<Health>(out var hp)) return;
        healthText.text = $"Core HP: {Mathf.Max(0, Mathf.CeilToInt(hp.Current))} / {Mathf.CeilToInt(hp.Max)}";

        if (_stateQuery.TryGetSingleton<GameStateSingleton>(out var s) && s.State == GameState.Lost)
        {
            Time.timeScale = 0f; // 또는 ECS World Update 중지 (선택)
        }
    }
}
