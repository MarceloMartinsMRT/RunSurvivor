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
    public bool isCharacterSelected = false;

    private GameObject gameOverPanel;
    private GameObject characterSelectPanel;
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
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null)
        {
            player = pObj.GetComponent<Player>();
        }

        // Abre o Menu Inicial de Seleção de Personagem
        ShowCharacterSelectUI();
    }

    void Update()
    {
        if (!isCharacterSelected) return;

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

    public void ShowCharacterSelectUI()
    {
        isCharacterSelected = false;
        GlobalVariables._gameIsPaused = true;
        Time.timeScale = 0f;

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("MainCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        characterSelectPanel = new GameObject("CharacterSelectPanel");
        characterSelectPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = characterSelectPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image bg = characterSelectPanel.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.05f, 0.08f, 0.96f);

        // Título Principal
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(characterSelectPanel.transform, false);
        TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "★ RUN! SURVIVOR ★";
        titleTMP.fontSize = 44;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(1f, 0.85f, 0.2f);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 175);
        titleRect.sizeDelta = new Vector2(650, 60);

        // Subtítulo
        GameObject subObj = new GameObject("SubtitleText");
        subObj.transform.SetParent(characterSelectPanel.transform, false);
        TextMeshProUGUI subTMP = subObj.AddComponent<TextMeshProUGUI>();
        subTMP.text = "SELECIONE SEU HERÓI INICIAL";
        subTMP.fontSize = 20;
        subTMP.fontStyle = FontStyles.Bold;
        subTMP.alignment = TextAlignmentOptions.Center;
        subTMP.color = new Color(0.8f, 0.95f, 1f, 0.9f);
        RectTransform subRect = subObj.GetComponent<RectTransform>();
        subRect.anchoredPosition = new Vector2(0, 130);
        subRect.sizeDelta = new Vector2(500, 35);

        // Container dos 2 Personagens
        CreateCharacterCard(
            characterSelectPanel.transform,
            new Vector2(-165, -20),
            "MAGO ARCANO",
            "Mestre Elemental",
            new Color(0.12f, 0.16f, 0.28f, 0.95f),
            new Color(0.25f, 0.65f, 1f, 0.95f),
            new Color(0.3f, 0.85f, 1f),
            "Arma Inicial: Hadouken\n\nDispara orbes de energia em linha reta que perfuram múltiplos inimigos.\n\n• Vida Máxima: 100 HP\n• Velocidade: 5.0 (Padrão)\n• Dano Geral: 100%\n• Acerto Crítico: 10%\n• Foco: Controle de rota e impacto",
            "JOGAR COM MAGO",
            0
        );

        CreateCharacterCard(
            characterSelectPanel.transform,
            new Vector2(165, -20),
            "MESTRE DAS LÂMINAS",
            "Assassino Veloz",
            new Color(0.08f, 0.22f, 0.14f, 0.95f),
            new Color(0.25f, 0.95f, 0.45f, 0.95f),
            new Color(0.35f, 1f, 0.55f),
            "Arma Inicial: Facas de Arremesso\n\nArremessa adagas velozes na direção de movimento do herói.\n\n• Vida Máxima: 85 HP\n• Velocidade: 6.0 (+20% Mais Rápido!)\n• Dano Geral: 100%\n• Acerto Crítico: 25% (+15% Inicial!)\n• Foco: Agilidade e golpes letais",
            "JOGAR COM LÂMINA",
            1
        );
    }

    private void CreateCharacterCard(
        Transform parent,
        Vector2 position,
        string heroName,
        string heroRole,
        Color bgColor,
        Color outlineColor,
        Color titleColor,
        string description,
        string buttonText,
        int heroIndex)
    {
        GameObject cardObj = new GameObject("HeroCard_" + heroIndex);
        cardObj.transform.SetParent(parent, false);

        RectTransform cardRect = cardObj.AddComponent<RectTransform>();
        cardRect.anchoredPosition = position;
        cardRect.sizeDelta = new Vector2(290, 360);

        Image cardBg = cardObj.AddComponent<Image>();
        cardBg.color = bgColor;

        Outline outline = cardObj.AddComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(2.5f, 2.5f);

        // Título do Herói
        GameObject titleObj = new GameObject("HeroTitle");
        titleObj.transform.SetParent(cardObj.transform, false);
        TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = heroName;
        titleTMP.fontSize = 20;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = titleColor;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 145);
        titleRect.sizeDelta = new Vector2(270, 35);

        // Subtítulo do Herói
        GameObject roleObj = new GameObject("HeroRole");
        roleObj.transform.SetParent(cardObj.transform, false);
        TextMeshProUGUI roleTMP = roleObj.AddComponent<TextMeshProUGUI>();
        roleTMP.text = heroRole;
        roleTMP.fontSize = 14;
        roleTMP.alignment = TextAlignmentOptions.Center;
        roleTMP.color = new Color(0.85f, 0.85f, 0.85f, 0.85f);
        RectTransform roleRect = roleObj.GetComponent<RectTransform>();
        roleRect.anchoredPosition = new Vector2(0, 120);
        roleRect.sizeDelta = new Vector2(270, 25);

        // Descrição e Stats
        GameObject descObj = new GameObject("HeroDesc");
        descObj.transform.SetParent(cardObj.transform, false);
        TextMeshProUGUI descTMP = descObj.AddComponent<TextMeshProUGUI>();
        descTMP.text = description;
        descTMP.fontSize = 13.5f;
        descTMP.lineSpacing = 10;
        descTMP.alignment = TextAlignmentOptions.Left;
        descTMP.color = new Color(0.95f, 0.95f, 0.95f);
        RectTransform descRect = descObj.GetComponent<RectTransform>();
        descRect.anchoredPosition = new Vector2(0, 5);
        descRect.sizeDelta = new Vector2(260, 180);

        // Botão Selecionar
        GameObject btnObj = new GameObject("SelectButton");
        btnObj.transform.SetParent(cardObj.transform, false);
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0, -140);
        btnRect.sizeDelta = new Vector2(250, 48);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = outlineColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = Color.white;
        cb.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        btn.colors = cb;

        Outline btnOutline = btnObj.AddComponent<Outline>();
        btnOutline.effectColor = Color.black;
        btnOutline.effectDistance = new Vector2(2, 2);

        GameObject btnTextObj = new GameObject("BtnText");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI btnTextTMP = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnTextTMP.text = buttonText;
        btnTextTMP.fontSize = 16;
        btnTextTMP.fontStyle = FontStyles.Bold;
        btnTextTMP.alignment = TextAlignmentOptions.Center;
        btnTextTMP.color = Color.black;
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchoredPosition = Vector2.zero;
        btnTextRect.sizeDelta = btnRect.sizeDelta;

        btn.onClick.AddListener(() =>
        {
            ChooseHero(heroIndex);
        });
    }

    private void ChooseHero(int heroIndex)
    {
        isCharacterSelected = true;

        if (player != null)
        {
            player.SetCharacter(heroIndex);
        }

        if (LevelUpManager.Instance != null)
        {
            LevelUpManager.Instance.SetupStartingWeapon(heroIndex);
        }

        if (characterSelectPanel != null)
        {
            Destroy(characterSelectPanel);
        }

        GlobalVariables._gameIsPaused = false;
        Time.timeScale = 1f;
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
