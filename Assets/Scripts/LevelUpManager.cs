using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance;

    [System.Serializable]
    public class UpgradeOption
    {
        public string id;
        public string title;
        public string description;
        public Action applyUpgrade;
    }

    private GameObject levelUpPanel;
    private Player player;
    private Power_Magic_Barrier magicBarrier;
    private Weapon_Aura holyAura;
    private Weapon_ThrowingKnives throwingKnives;
    private Weapon_MeteorRain meteorRain;
    private Weapon_Whip whip;

    private int hadoukenLevel = 1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<Player>();
            magicBarrier = playerObj.GetComponentInChildren<Power_Magic_Barrier>();
            holyAura = playerObj.GetComponentInChildren<Weapon_Aura>();
            throwingKnives = playerObj.GetComponentInChildren<Weapon_ThrowingKnives>();
            meteorRain = playerObj.GetComponentInChildren<Weapon_MeteorRain>();
            whip = playerObj.GetComponentInChildren<Weapon_Whip>();

            // Instancia armas como componentes no Player se ainda não existirem
            if (magicBarrier == null)
            {
                GameObject barrierObj = new GameObject("Power_Magic_Barrier");
                barrierObj.transform.SetParent(playerObj.transform);
                barrierObj.transform.localPosition = Vector3.zero;
                magicBarrier = barrierObj.AddComponent<Power_Magic_Barrier>();
            }

            if (holyAura == null)
            {
                GameObject auraObj = new GameObject("Weapon_Aura");
                auraObj.transform.SetParent(playerObj.transform);
                auraObj.transform.localPosition = Vector3.zero;
                holyAura = auraObj.AddComponent<Weapon_Aura>();
            }

            if (throwingKnives == null)
            {
                GameObject knivesObj = new GameObject("Weapon_ThrowingKnives");
                knivesObj.transform.SetParent(playerObj.transform);
                knivesObj.transform.localPosition = Vector3.zero;
                throwingKnives = knivesObj.AddComponent<Weapon_ThrowingKnives>();
            }

            if (meteorRain == null)
            {
                GameObject meteorObj = new GameObject("Weapon_MeteorRain");
                meteorObj.transform.SetParent(playerObj.transform);
                meteorObj.transform.localPosition = Vector3.zero;
                meteorRain = meteorObj.AddComponent<Weapon_MeteorRain>();
            }

            if (whip == null)
            {
                GameObject whipObj = new GameObject("Weapon_Whip");
                whipObj.transform.SetParent(playerObj.transform);
                whipObj.transform.localPosition = Vector3.zero;
                whip = whipObj.AddComponent<Weapon_Whip>();
            }
        }
    }

    public void ShowLevelUp()
    {
        GlobalVariables._gameIsPaused = true;
        Time.timeScale = 0f;

        List<UpgradeOption> availableUpgrades = GetAvailableUpgrades();
        // Embaralha e pega até 3 opções aleatórias
        Shuffle(availableUpgrades);
        List<UpgradeOption> selected = availableUpgrades.GetRange(0, Mathf.Min(3, availableUpgrades.Count));

        DisplayUI(selected);
    }

    private List<UpgradeOption> GetAvailableUpgrades()
    {
        List<UpgradeOption> list = new List<UpgradeOption>();

        // 1. Hadouken
        if (hadoukenLevel < 5)
        {
            list.Add(new UpgradeOption
            {
                id = "hadouken",
                title = "Hadouken Lv " + (hadoukenLevel + 1),
                description = hadoukenLevel % 2 == 1 ? "+1 Projétil extra em leque!" : "+30% Dano de Hadouken e recarga mais rápida!",
                applyUpgrade = () =>
                {
                    hadoukenLevel++;
                    if (hadoukenLevel % 2 == 0)
                        player.AddExtraProjectile(1);
                    else
                    {
                        player.increasePower(1.5f);
                        player.setFireRate(20f);
                    }
                }
            });
        }

        // 2. Barreira Mágica (Orbes Giratórias)
        int barrierLv = (magicBarrier != null) ? magicBarrier.level : 0;
        if (barrierLv < 5)
        {
            list.Add(new UpgradeOption
            {
                id = "barrier",
                title = barrierLv == 0 ? "Nova Arma: Barreira Mágica" : "Barreira Mágica Lv " + (barrierLv + 1),
                description = barrierLv == 0 ? "Orbe de energia protetora que gira em torno de você." : "+1 Orbe giratória e maior velocidade de rotação!",
                applyUpgrade = () =>
                {
                    if (magicBarrier != null) magicBarrier.UnlockOrUpgrade();
                }
            });
        }

        // 3. Aura Sagrada (Dano Contínuo ao Redor)
        int auraLv = (holyAura != null) ? holyAura.level : 0;
        if (auraLv < 5)
        {
            list.Add(new UpgradeOption
            {
                id = "aura",
                title = auraLv == 0 ? "Nova Arma: Aura Sagrada" : "Aura Sagrada Lv " + (auraLv + 1),
                description = auraLv == 0 ? "Anel de energia sagrada que queima continuamente inimigos próximos." : "+25% Área e dano ampliado da Aura!",
                applyUpgrade = () =>
                {
                    if (holyAura != null) holyAura.UnlockOrUpgrade();
                }
            });
        }

        // 4. Facas de Arremesso (Disparo na Direção do Movimento)
        int knivesLv = (throwingKnives != null) ? throwingKnives.level : 0;
        if (knivesLv < 5)
        {
            list.Add(new UpgradeOption
            {
                id = "knives",
                title = knivesLv == 0 ? "Nova Arma: Facas de Arremesso" : "Facas de Arremesso Lv " + (knivesLv + 1),
                description = knivesLv == 0 ? "Arremessa adagas velozes na direção do seu movimento/visão." : "+1 Faca em leque, perfuração de inimigos e mais dano!",
                applyUpgrade = () =>
                {
                    if (throwingKnives != null) throwingKnives.UnlockOrUpgrade();
                }
            });
        }

        // 5. Chuva de Meteoros / Raios (Ataque Celestial em Área)
        int meteorLv = (meteorRain != null) ? meteorRain.level : 0;
        if (meteorLv < 5)
        {
            list.Add(new UpgradeOption
            {
                id = "meteor",
                title = meteorLv == 0 ? "Nova Arma: Fúria Celestial" : "Fúria Celestial Lv " + (meteorLv + 1),
                description = meteorLv == 0 ? "Chuva contínua de meteoros devastadores sobre inimigos na tela." : "+1 Meteoro por queda, maior raio de impacto e dano explosivo!",
                applyUpgrade = () =>
                {
                    if (meteorRain != null) meteorRain.UnlockOrUpgrade();
                }
            });
        }

        // 6. Chicote Frontal (Corte Amplo em Arco)
        int whipLv = (whip != null) ? whip.level : 0;
        if (whipLv < 5)
        {
            list.Add(new UpgradeOption
            {
                id = "whip",
                title = whipLv == 0 ? "Nova Arma: Chicote Sagrado" : "Chicote Sagrado Lv " + (whipLv + 1),
                description = whipLv == 0 ? "Golpe horizontal devastador à frente que repele inimigos em massa." : (whipLv == 2 ? "Chicote Duplo! Golpeia à frente E atrás!" : "+35% Dano, maior alcance e knockback reforçado!"),
                applyUpgrade = () =>
                {
                    if (whip != null) whip.UnlockOrUpgrade();
                }
            });
        }

        // 7. Melhorias Passivas
        list.Add(new UpgradeOption
        {
            id = "boots",
            title = "Botas de Agilidade",
            description = "+15% Velocidade de Movimento permanente.",
            applyUpgrade = () => player.IncreaseSpeed(15f)
        });

        list.Add(new UpgradeOption
        {
            id = "damage",
            title = "Tomo do Poder",
            description = "+20% Dano em TODAS as armas.",
            applyUpgrade = () => player.IncreaseDamage(20f)
        });

        list.Add(new UpgradeOption
        {
            id = "crit_chance",
            title = "Olho de Águia",
            description = "+8% de Chance de Acerto Crítico em todos os ataques.",
            applyUpgrade = () => player.IncreaseCritChance(8f)
        });

        list.Add(new UpgradeOption
        {
            id = "crit_mult",
            title = "Lâmina Vorpal",
            description = "+40% de Multiplicador de Dano Crítico.",
            applyUpgrade = () => player.IncreaseCritMultiplier(0.40f)
        });

        list.Add(new UpgradeOption
        {
            id = "cooldown",
            title = "Ampulheta Rápida",
            description = "-12% Tempo de recarga de todas as habilidades.",
            applyUpgrade = () => player.ReduceCooldown(12f)
        });

        list.Add(new UpgradeOption
        {
            id = "magnet",
            title = "Ímã Atraente",
            description = "+35% Raio de atração de gemas de XP.",
            applyUpgrade = () => player.IncreaseMagnetRadius(35f)
        });

        list.Add(new UpgradeOption
        {
            id = "heart",
            title = "Coração de Titã",
            description = "+25 Vida Máxima e cura 50 HP instantaneamente.",
            applyUpgrade = () =>
            {
                player.IncreaseMaxHealth(25f);
                player.Heal(50f);
            }
        });

        return list;
    }

    private void DisplayUI(List<UpgradeOption> options)
    {
        if (levelUpPanel != null) Destroy(levelUpPanel);

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("LevelUpCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        levelUpPanel = new GameObject("LevelUpPanel");
        levelUpPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = levelUpPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Fundo escuro semi-transparente
        Image bg = levelUpPanel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.75f);

        // Título "★ LEVEL UP! ★"
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(levelUpPanel.transform, false);
        TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "★ LEVEL UP! ★";
        titleTMP.fontSize = 42;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(1f, 0.85f, 0.2f);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 160);
        titleRect.sizeDelta = new Vector2(500, 60);

        // Container para os 3 cartões
        float cardWidth = 260f;
        float cardHeight = 180f;
        float spacing = 20f;
        float totalWidth = (cardWidth * options.Count) + (spacing * (options.Count - 1));
        float startX = -totalWidth / 2f + (cardWidth / 2f);

        for (int i = 0; i < options.Count; i++)
        {
            UpgradeOption opt = options[i];
            float posX = startX + i * (cardWidth + spacing);

            GameObject cardObj = new GameObject("Card_" + i);
            cardObj.transform.SetParent(levelUpPanel.transform, false);
            RectTransform cardRect = cardObj.AddComponent<RectTransform>();
            cardRect.anchoredPosition = new Vector2(posX, -20);
            cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);

            Image cardBg = cardObj.AddComponent<Image>();
            cardBg.color = new Color(0.15f, 0.18f, 0.25f, 0.95f);

            Button btn = cardObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.25f, 0.35f, 0.55f, 1f);
            cb.pressedColor = new Color(0.1f, 0.5f, 0.8f, 1f);
            btn.colors = cb;

            // Borda colorida
            Outline outline = cardObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.6f, 0.9f, 0.8f);
            outline.effectDistance = new Vector2(2, 2);

            // Nome da melhoria
            GameObject optTitleObj = new GameObject("CardTitle");
            optTitleObj.transform.SetParent(cardObj.transform, false);
            TextMeshProUGUI optTitle = optTitleObj.AddComponent<TextMeshProUGUI>();
            optTitle.text = opt.title;
            optTitle.fontSize = 20;
            optTitle.fontStyle = FontStyles.Bold;
            optTitle.alignment = TextAlignmentOptions.Center;
            optTitle.color = new Color(1f, 0.9f, 0.4f);
            RectTransform optTitleRect = optTitleObj.GetComponent<RectTransform>();
            optTitleRect.anchoredPosition = new Vector2(0, 50);
            optTitleRect.sizeDelta = new Vector2(cardWidth - 20, 45);

            // Descrição da melhoria
            GameObject descObj = new GameObject("CardDesc");
            descObj.transform.SetParent(cardObj.transform, false);
            TextMeshProUGUI optDesc = descObj.AddComponent<TextMeshProUGUI>();
            optDesc.text = opt.description;
            optDesc.fontSize = 15;
            optDesc.alignment = TextAlignmentOptions.Center;
            optDesc.color = Color.white;
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchoredPosition = new Vector2(0, -20);
            descRect.sizeDelta = new Vector2(cardWidth - 30, 80);

            // Clique do botão
            btn.onClick.AddListener(() =>
            {
                SelectUpgrade(opt);
            });
        }
    }

    private void SelectUpgrade(UpgradeOption option)
    {
        if (option != null && option.applyUpgrade != null)
        {
            option.applyUpgrade();
        }

        if (levelUpPanel != null)
        {
            Destroy(levelUpPanel);
        }

        GlobalVariables._gameIsPaused = false;
        Time.timeScale = 1f;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = UnityEngine.Random.Range(0, i + 1);
            T tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }
}
