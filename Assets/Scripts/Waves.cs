using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Waves : MonoBehaviour
{
    [Header("Wave State")]
    public int waveCount = 1;
    public int enemiesKilledTotal = 0;
    public float timeElapsed = 0f;

    [Header("UI References")]
    [SerializeField] private Image exp;
    [SerializeField] private TextMeshProUGUI waveText;

    private Spawn _spawn;
    private float waveTimer = 0f;
    private float waveDuration = 30f; // Cada onda dura 30 segundos

    void Start()
    {
        _spawn = FindObjectOfType<Spawn>();
        UpdateWaveUI();
    }

    void Update()
    {
        if (GlobalVariables._gameIsPaused) return;

        timeElapsed += Time.deltaTime;
        waveTimer += Time.deltaTime;

        if (waveTimer >= waveDuration)
        {
            NextWave();
        }

        UpdateWaveUI();
    }

    void NextWave()
    {
        waveTimer = 0f;
        waveCount++;

        // Aumenta a intensidade no spawner
        if (_spawn != null)
        {
            _spawn.minSpawnAmount = 5 + waveCount * 2;
            _spawn.maxSpawnAmount = 10 + waveCount * 3;
            _spawn.spawnInterval = Mathf.Max(1.2f, 3.5f - (waveCount * 0.2f));

            // Dispara um pequeno enxame no início da nova onda
            _spawn.SpawnEnemies(8 + waveCount * 2, waveCount * 15);
        }
    }

    public void EnemyKilled()
    {
        enemiesKilledTotal++;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddKill();
        }
    }

    private void UpdateWaveUI()
    {
        if (waveText != null)
        {
            int minutes = Mathf.FloorToInt(timeElapsed / 60F);
            int seconds = Mathf.FloorToInt(timeElapsed % 60F);
            waveText.text = string.Format("Onda {0} | {1:00}:{2:00}", waveCount, minutes, seconds);
        }
    }

    public void SetWaveNumberText(int waveNumber)
    {
        waveCount = waveNumber;
        UpdateWaveUI();
    }
}
