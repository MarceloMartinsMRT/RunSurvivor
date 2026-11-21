using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int totalKills = 0;
    public float survivalTime = 0f;
    public bool isGameOver = false;

    private GameObject gameOverPanel;
    private Player player;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Garante que o LevelUpManager existe
        if (FindObjectOfType<LevelUpManager>() == null)
        {
            gameObject.AddComponent<LevelUpManager>();
        }
    }

    void Start()
    {
        Time.timeScale = 1f;
        GlobalVariables._gameIsPaused = false;

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null)
        {
            player = pObj.GetComponent<Player>();
        }
    }

    void Update()
    {
        if (isGameOver)
        {
            // Atalho de teclado para reiniciar rápido
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
            {
                RestartGame();
            }
            return;
        }

        if (!GlobalVariables._gameIsPaused)
        {
            survivalTime += Time.deltaTime;
        }
    }

    public void AddKill()
    {
        totalKills++;
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        GlobalVariables._gameIsPaused = true;
        Time.timeScale = 0f;

        ShowGameOverUI();
    }

    private void ShowGameOverUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("GameOverCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = gameOverPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image bg = gameOverPanel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.02f, 0.02f, 0.88f);

        // Container Central
        GameObject container = new GameObject("Container");
        container.transform.SetParent(gameOverPanel.transform, false);
        RectTransform cRect = container.AddComponent<RectTransform>();
        cRect.sizeDelta = new Vector2(450, 380);

        Image cBg = container.AddComponent<Image>();
        cBg.color = new Color(0.18f, 0.05f, 0.05f, 0.95f);
        Outline outline = container.AddComponent<Outline>();
        outline.effectColor = new Color(0.9f, 0.2f, 0.2f, 0.8f);
        outline.effectDistance = new Vector2(3, 3);

        // Título GAME OVER
        GameObject titleObj = new GameObject("GameOverTitle");
        titleObj.transform.SetParent(container.transform, false);
        TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "VOCÊ FOI DERROTADO";
        titleTMP.fontSize = 32;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(1f, 0.25f, 0.25f);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 130);
        titleRect.sizeDelta = new Vector2(400, 50);

        // Estatísticas
        int minutes = Mathf.FloorToInt(survivalTime / 60F);
        int seconds = Mathf.FloorToInt(survivalTime % 60F);
        int playerLevel = (player != null) ? player.currentLevel : 1;

        string statsText = string.Format(
            "Tempo Sobrevivido: {0:00}:{1:00}\nInimigos Derrotados: {2}\nNível Alcançado: {3}",
            minutes, seconds, totalKills, playerLevel
        );

        GameObject statsObj = new GameObject("StatsText");
        statsObj.transform.SetParent(container.transform, false);
        TextMeshProUGUI statsTMP = statsObj.AddComponent<TextMeshProUGUI>();
        statsTMP.text = statsText;
        statsTMP.fontSize = 20;
        statsTMP.lineSpacing = 15;
        statsTMP.alignment = TextAlignmentOptions.Center;
        statsTMP.color = Color.white;
        RectTransform statsRect = statsObj.GetComponent<RectTransform>();
        statsRect.anchoredPosition = new Vector2(0, 20);
        statsRect.sizeDelta = new Vector2(380, 120);

        // Botão Reiniciar
        GameObject btnObj = new GameObject("RestartButton");
        btnObj.transform.SetParent(container.transform, false);
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0, -110);
        btnRect.sizeDelta = new Vector2(240, 55);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.85f, 0.2f, 0.2f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(1f, 0.35f, 0.35f, 1f);
        cb.pressedColor = new Color(0.6f, 0.1f, 0.1f, 1f);
        btn.colors = cb;

        GameObject btnTextObj = new GameObject("BtnText");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI btnTMP = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnTMP.text = "JOGAR NOVAMENTE [R]";
        btnTMP.fontSize = 18;
        btnTMP.fontStyle = FontStyles.Bold;
        btnTMP.alignment = TextAlignmentOptions.Center;
        btnTMP.color = Color.white;
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.sizeDelta = btnRect.sizeDelta;

        btn.onClick.AddListener(RestartGame);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        GlobalVariables._gameIsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
