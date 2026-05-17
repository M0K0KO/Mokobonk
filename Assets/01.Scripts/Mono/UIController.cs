using TMPro;
using Unity.Entities;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text coreHealthText;
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private GameObject gameOverPanel;

    private EntityManager _entityManager;
    private EntityQuery _waveQuery, _resQuery, _coreQuery, _stateQuery;

    private void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _waveQuery = _entityManager.CreateEntityQuery(typeof(WaveStateSingleton));
        _resQuery = _entityManager.CreateEntityQuery(typeof(ResourceSingleton));
        _coreQuery = _entityManager.CreateEntityQuery(typeof(Health), typeof(CoreTag));
        _stateQuery = _entityManager.CreateEntityQuery(typeof(GameStateSingleton));
    }

    private void Update()
    {
        if (_waveQuery.CalculateEntityCount() > 0)
        {
            var w = _waveQuery.GetSingleton<WaveStateSingleton>();
            waveText.text = $"Wave {w.CurrentWave}";
            phaseText.text = w.Phase.ToString();
        }
        if (_resQuery.CalculateEntityCount() > 0)
        {
            var r = _resQuery.GetSingleton<ResourceSingleton>();
            goldText.text = $"Gold: {r.Gold}";
        }
        if (_coreQuery.CalculateEntityCount() > 0)
        {
            var hp = _coreQuery.GetSingleton<Health>();
            coreHealthText.text = $"Core: {Mathf.Max(0, Mathf.CeilToInt(hp.Current))} / {Mathf.CeilToInt(hp.Max)}";
        }
        if (_stateQuery.CalculateEntityCount() > 0)
        {
            var s = _stateQuery.GetSingleton<GameStateSingleton>();
            if (s.State == GameState.Lost && !gameOverPanel.activeSelf)
            {
                gameOverPanel.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }
}