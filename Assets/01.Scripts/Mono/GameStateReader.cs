using Unity.Entities;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public sealed class GameStateReader : MonoBehaviour
{
    public static GameStateReader Instance { get; private set; }

    private EntityManager _em;
    private EntityQuery _waveQuery;
    private EntityQuery _phaseQuery;
    private EntityQuery _resQuery;
    private EntityQuery _coreQuery;
    private EntityQuery _stateQuery;

    public GamePhase Phase { get; private set; }
    public float PhaseElapsed { get; private set; }
    public WavePhase WavePhase { get; private set; }
    public int CurrentWave { get; private set; }
    public int RemainingEnemies { get; private set; }
    public float TimeToNextWave { get; private set; }
    public int Gold { get; private set; }
    public int CoreHealth { get; private set; }
    public int CoreMaxHealth { get; private set; }
    public GameState GameState { get; private set; }

    public bool IsInPreparation => Phase == GamePhase.Preparation;
    public bool IsInHorde => Phase == GamePhase.Horde;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
        _waveQuery = _em.CreateEntityQuery(typeof(WaveStateSingleton));
        _phaseQuery = _em.CreateEntityQuery(typeof(GamePhaseSingleton));
        _resQuery = _em.CreateEntityQuery(typeof(ResourceSingleton));
        _coreQuery = _em.CreateEntityQuery(typeof(Health), typeof(CoreTag));
        _stateQuery = _em.CreateEntityQuery(typeof(GameStateSingleton));
    }

    void Update()
    {
        float now = (float)World.DefaultGameObjectInjectionWorld.Time.ElapsedTime;

        if (!_waveQuery.IsEmpty)
        {
            var w = _waveQuery.GetSingleton<WaveStateSingleton>();
            WavePhase = w.Phase;
            CurrentWave = w.CurrentWave;
            RemainingEnemies = w.RemainingEnemies;
            TimeToNextWave = Mathf.Max(0f, w.NextWaveTime - now);
        }

        if (!_phaseQuery.IsEmpty)
        {
            var p = _phaseQuery.GetSingleton<GamePhaseSingleton>();
            Phase = p.Phase;
            PhaseElapsed = p.PhaseElapsed;
        }

        if (!_resQuery.IsEmpty)
            Gold = _resQuery.GetSingleton<ResourceSingleton>().Gold;

        if (!_coreQuery.IsEmpty)
        {
            var hp = _coreQuery.GetSingleton<Health>();
            CoreHealth = Mathf.Max(0, Mathf.CeilToInt(hp.Current));
            CoreMaxHealth = Mathf.CeilToInt(hp.Max);
        }

        if (!_stateQuery.IsEmpty)
            GameState = _stateQuery.GetSingleton<GameStateSingleton>().State;
    }
}