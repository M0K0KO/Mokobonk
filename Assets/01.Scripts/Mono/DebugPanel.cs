using System;
using Unity.Entities;
using UnityEngine;

public class DebugPanel : MonoBehaviour
{
    private bool _open;
    private EntityManager _em;
    private GUIStyle _windowStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _buttonStyle;
    private bool _stylesInit;

    [SerializeField] private float _maxTimeScale = 5f;

    void Awake() => _em = World.DefaultGameObjectInjectionWorld.EntityManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) _open = !_open;

        if (Input.GetKeyDown(KeyCode.Alpha1)) Time.timeScale = 1f;
        if (Input.GetKeyDown(KeyCode.Alpha2)) Time.timeScale = 2f;
        if (Input.GetKeyDown(KeyCode.Alpha3)) Time.timeScale = 3f;
        if (Input.GetKeyDown(KeyCode.Alpha4)) Time.timeScale = 5f;
        if (Input.GetKeyDown(KeyCode.Alpha0)) Time.timeScale = 0f;
    }

    private void InitStyles()
    {
        if (_stylesInit) return;
        _stylesInit = true;

        // 4K에서 보일 만한 크기
        _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 28 };
        _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 28, fixedHeight = 50 };
        _windowStyle = new GUIStyle(GUI.skin.window) { fontSize = 28 };
    }

    void OnGUI()
    {
        if (!_open) return;
        InitStyles();

        var r = GameStateReader.Instance;
        if (r == null) return;

        GUILayout.BeginArea(new Rect(20, 20, 600, 1100), _windowStyle);
        GUILayout.Label("=== Debug Panel (F1) ===", _labelStyle);
        GUILayout.Label($"Phase: {r.Phase}  (Wave {r.CurrentWave})", _labelStyle);
        GUILayout.Label($"WavePhase: {r.WavePhase}", _labelStyle);
        GUILayout.Label($"Remaining: {r.RemainingEnemies}", _labelStyle);
        GUILayout.Label($"NextWave: {r.TimeToNextWave:F1}s", _labelStyle);
        GUILayout.Label($"Gold: {r.Gold}", _labelStyle);
        GUILayout.Label($"Core: {r.CoreHealth} / {r.CoreMaxHealth}", _labelStyle);



        GUILayout.Space(20);
        if (GUILayout.Button("Force Horde", _buttonStyle)) ForceTransition(GamePhase.Horde);
        if (GUILayout.Button("Force Preparation", _buttonStyle)) ForceTransition(GamePhase.Preparation);
        if (GUILayout.Button("+100000 Gold", _buttonStyle)) AddGold(100000);



        GUILayout.Space(20);
        GUILayout.Label("--- Time ---", _labelStyle);
        GUILayout.Label($"TimeScale: {Time.timeScale:F2}x", _labelStyle);

        float newScale = GUILayout.HorizontalSlider(Time.timeScale, 0f, _maxTimeScale, GUILayout.Width(560));
        if (Mathf.Abs(newScale - Time.timeScale) > 0.01f) Time.timeScale = newScale;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("0x (Pause)", _buttonStyle)) Time.timeScale = 0f;
        if (GUILayout.Button("1x", _buttonStyle)) Time.timeScale = 1f;
        if (GUILayout.Button("2x", _buttonStyle)) Time.timeScale = 2f;
        if (GUILayout.Button("3x", _buttonStyle)) Time.timeScale = 3f;
        if (GUILayout.Button("5x", _buttonStyle)) Time.timeScale = 5f;
        GUILayout.EndHorizontal();


        GUILayout.Space(20);
        GUILayout.Label("--- Balance ---", _labelStyle);

        DrawBalanceSliders();


        GUILayout.EndArea();
    }

    private void DrawBalanceSliders()
    {
        var query = _em.CreateEntityQuery(typeof(BalanceMultiplierSingleton));
        if (query.IsEmpty) return;

        var bal = query.GetSingleton<BalanceMultiplierSingleton>();
        bool changed = false;

        bal.EnemyHpMul = LabeledSlider("Enemy HP", bal.EnemyHpMul, 0.3f, 5f, ref changed);
        bal.EnemySpeedMul = LabeledSlider("Enemy Speed", bal.EnemySpeedMul, 0.3f, 3f, ref changed);
        bal.EnemyDamageMul = LabeledSlider("Enemy Damage", bal.EnemyDamageMul, 0.3f, 3f, ref changed);
        bal.SpawnRateMul = LabeledSlider("Spawn Rate", bal.SpawnRateMul, 0.3f, 5f, ref changed);
        bal.WalkerRatio = LabeledSlider("Walker Ratio", bal.WalkerRatio, 0.0f, 1f, ref changed);
        bal.TurretDamageMul = LabeledSlider("Turret Damage", bal.TurretDamageMul, 0.3f, 5f, ref changed);
        bal.TurretFireRateMul = LabeledSlider("Turret FireRate", bal.TurretFireRateMul, 0.3f, 5f, ref changed);
        bal.TurretRangeMul = LabeledSlider("Turret Range", bal.TurretRangeMul, 0.5f, 3f, ref changed);

        if (changed)
        {
            query.SetSingleton(bal);
        }

        if (GUILayout.Button("Reset Balance to 1.0", _buttonStyle))
        {
            query.SetSingleton(new BalanceMultiplierSingleton
            {
                EnemyHpMul = 1f,
                EnemySpeedMul = 1f,
                EnemyDamageMul = 1f,
                SpawnRateMul = 1f,
                WalkerRatio = 0.2f,
                TurretDamageMul = 1f,
                TurretFireRateMul = 1f,
                TurretRangeMul = 1f,
            });
        }
    }

    private float LabeledSlider(string label, float value, float min, float max, ref bool changed)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{label}: {value:F2}x", _labelStyle, GUILayout.Width(280));
        float newVal = GUILayout.HorizontalSlider(value, min, max, GUILayout.Width(280));
        GUILayout.EndHorizontal();
        if (Mathf.Abs(newVal - value) > 0.001f) changed = true;
        return newVal;
    }

    private void ForceTransition(GamePhase target)
    {
        var e = _em.CreateEntity();
        _em.AddComponentData(e, new ForcePhaseTransition { Target = target });
    }

    private void AddGold(int amount)
    {
        var q = _em.CreateEntityQuery(typeof(ResourceSingleton));
        if (q.IsEmpty) return;
        var r = q.GetSingleton<ResourceSingleton>();
        r.Gold += amount;
        q.SetSingleton(r);
    }
}