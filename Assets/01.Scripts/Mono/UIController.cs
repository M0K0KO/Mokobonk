using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("HUD Texts")]
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text coreHealthText;
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text remainingText;

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;

    private GameStateReader _reader;
    private bool _gameOverShown;

    void Start()
    {
        _reader = GameStateReader.Instance;
    }

    void Update()
    {
        if (_reader == null) return;

        waveText.text = $"Wave {_reader.CurrentWave}";

        phaseText.text = _reader.Phase == GamePhase.Preparation ? "Preparation" : "Horde";
        goldText.text = $"Gold: {_reader.Gold}";
        coreHealthText.text = $"Core: {_reader.CoreHealth} / {_reader.CoreMaxHealth}";

        if (_reader.IsInPreparation)
        {
            countdownText.gameObject.SetActive(true);
            remainingText.gameObject.SetActive(false);
            countdownText.text = $"Next Wave: {_reader.TimeToNextWave:F1}s";
        }
        else
        {
            countdownText.gameObject.SetActive(false);
            remainingText.gameObject.SetActive(true);
            remainingText.text = $"Enemies: {_reader.RemainingEnemies}";
        }

        if (_reader.GameState == GameState.Lost && !_gameOverShown)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
            _gameOverShown = true;
        }
    }
}