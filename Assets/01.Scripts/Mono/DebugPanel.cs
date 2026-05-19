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

    void Awake() => _em = World.DefaultGameObjectInjectionWorld.EntityManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) _open = !_open;
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

        GUILayout.BeginArea(new Rect(20, 20, 600, 800), _windowStyle);
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
        if (GUILayout.Button("+100 Gold", _buttonStyle)) AddGold(100);

        GUILayout.EndArea();
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