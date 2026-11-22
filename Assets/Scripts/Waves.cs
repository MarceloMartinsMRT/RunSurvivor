using System.Collections;
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

    private bool elite90sSpawned = false;
    private bool boss3MinSpawned = false;
    private bool boss5MinSpawned = false;
    private string currentAlertMessage = "";
    private float alertDisplayTimer = 0f;

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

        CheckBossSpawns();

        // Atalho de Depuração / Teste rápido: Pressione a tecla B para gerar um Chefe instantaneamente!
        if (Input.GetKeyDown(KeyCode.B))
        {
            SpawnBoss("★ CHEFE DE TESTE ★", 400, 22, 2.0f, 2.2f, 3);
            ShowBossAlert("⚠ CHEFE DE TESTE INVOCADO! ⚠");
        }

        if (alertDisplayTimer > 0f)
        {
            alertDisplayTimer -= Time.deltaTime;
        }

        UpdateWaveUI();
    }

    private void CheckBossSpawns()
    {
        // 1:30 (90 segundos) - Inimigo Elite Mini-Chefe (Baú com 1 a 2 upgrades)
        if (!elite90sSpawned && timeElapsed >= 90f)
        {
            elite90sSpawned = true;
            SpawnBoss("★ MONSTRO ELITE ★", 280, 18, 1.7f, 1.7f, Random.Range(1, 3));
            ShowBossAlert("⚠ ALERTA: MONSTRO ELITE DETECTADO! ⚠");
        }

        // 3:00 (180 segundos) - Primeiro Grande Chefe Gigante (Baú com 3 upgrades)
        if (!boss3MinSpawned && timeElapsed >= 180f)
        {
            boss3MinSpawned = true;
            SpawnBoss("★ REI GOBLIN TITÃ ★", 650, 26, 1.7f, 2.3f, 3);
            ShowBossAlert("⚠ ALERTA DE CHEFE: REI GOBLIN TITÃ SURGIU! ⚠");
        }

        // 5:00 (300 segundos) - Segundo Chefe Colossal (Baú com 3 upgrades)
        if (!boss5MinSpawned && timeElapsed >= 300f)
        {
            boss5MinSpawned = true;
            SpawnBoss("★ COLOSSO DA MORTE ★", 1600, 36, 1.9f, 2.8f, 3);
            ShowBossAlert("⚠ ALERTA MÁXIMO: COLOSSO DA MORTE SURGIU! ⚠");
        }
    }

    public void SpawnBoss(string title, int health, int damage, float speed, float scale, int chestUpgrades)
    {
        if (_spawn != null)
        {
            _spawn.SpawnBoss(title, health, damage, speed, scale, chestUpgrades);
        }
    }

    public void ShowBossAlert(string message)
    {
        currentAlertMessage = message;
        alertDisplayTimer = 4.0f;
        StartCoroutine(SpawnBossAlertBanner(message));
    }

    private IEnumerator SpawnBossAlertBanner(string message)
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) yield break;

        GameObject bannerObj = new GameObject("BossAlertBanner");
        bannerObj.transform.SetParent(canvas.transform, false);

        RectTransform rect = bannerObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0, -90);
        rect.sizeDelta = new Vector2(650, 50);

        Image bg = bannerObj.AddComponent<Image>();
        bg.color = new Color(0.7f, 0.1f, 0.1f, 0.75f);

        Outline outline = bannerObj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.2f, 0.95f);
        outline.effectDistance = new Vector2(2, 2);

        GameObject textObj = new GameObject("BannerText");
        textObj.transform.SetParent(bannerObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = message;
        tmp.fontSize = 22;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.95f, 0.3f);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(640, 50);

        float duration = 4.0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            // Efeito de pulso suave
            float pulse = 1f + Mathf.Sin(elapsed * 8f) * 0.05f;
            bannerObj.transform.localScale = new Vector3(pulse, pulse, 1f);
            yield return null;
        }

        Destroy(bannerObj);
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

            if (alertDisplayTimer > 0f)
            {
                waveText.text = $"<color=#FF4444>{currentAlertMessage}</color>";
            }
            else
            {
                waveText.text = string.Format("Onda {0} | {1:00}:{2:00}", waveCount, minutes, seconds);
            }
        }
    }

    public void SetWaveNumberText(int waveNumber)
    {
        waveCount = waveNumber;
        UpdateWaveUI();
    }
}
