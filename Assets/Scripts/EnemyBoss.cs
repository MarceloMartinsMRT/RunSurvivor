using System.Collections;
using TMPro;
using UnityEngine;

public class EnemyBoss : Enemy
{
    [Header("Boss / Elite Settings")]
    public string bossTitle = "★ REI GOBLIN TITÃ ★";
    public int chestUpgrades = 3; // 1 a 3 upgrades no baú de tesouro
    public float chargeCooldown = 5.5f;

    private bool isCharging = false;
    private float chargeTimer = 0f;
    private float chargeDuration = 1.3f;
    private float currentChargeDurationTimer = 0f;

    private GameObject healthBarObj;
    private SpriteRenderer hpFillBar;
    private TextMeshPro hpTextTMP;
    private Color bossNormalColor = new Color(1f, 0.45f, 0.45f, 1f);

    protected override void Awake()
    {
        base.Awake();
        knockbackResistance = 0.85f; // Resiste a 85% de qualquer empurrão
    }

    protected override void Start()
    {
        base.Start();

        // Tom avermelhado imponente
        if (spriteRenderer != null)
        {
            spriteRenderer.color = bossNormalColor;
        }

        CreateWorldHealthBar();
        UpdateHealthBarUI();
    }

    protected override void FixedUpdate()
    {
        if (GlobalVariables._gameIsPaused || _isDead || playerTransform == null) return;

        chargeTimer += Time.fixedDeltaTime;

        if (isCharging)
        {
            currentChargeDurationTimer -= Time.fixedDeltaTime;

            // Avanço veloz contínuo na direção do jogador
            Vector2 toPlayer = (Vector2)playerTransform.position - rb.position;
            Vector2 dir = toPlayer.normalized;
            rb.MovePosition(rb.position + dir * (_originalSpeed * 2.2f) * Time.fixedDeltaTime);

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = dir.x < 0;
            }

            if (currentChargeDurationTimer <= 0f)
            {
                isCharging = false;
                chargeTimer = 0f;
                if (spriteRenderer != null) spriteRenderer.color = bossNormalColor;
            }

            if (toPlayer.sqrMagnitude <= 1.5f)
            {
                TryDealContactDamage();
            }
        }
        else
        {
            base.FixedUpdate();

            if (chargeTimer >= chargeCooldown)
            {
                StartCoroutine(TriggerRageCharge());
            }
        }

        UpdateHealthBarPosition();
    }

    private IEnumerator TriggerRageCharge()
    {
        chargeTimer = 0f;

        // Feedback de preparação / fúria antes da investida
        if (spriteRenderer != null)
        {
            for (int i = 0; i < 3; i++)
            {
                spriteRenderer.color = new Color(1f, 0.1f, 0.1f, 1f);
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }
            spriteRenderer.color = new Color(1f, 0.2f, 0.2f, 1f);
        }

        isCharging = true;
        currentChargeDurationTimer = chargeDuration;
    }

    public override void TakeDamage(int damage, Vector2 knockbackDir = default, bool isCrit = false)
    {
        base.TakeDamage(damage, knockbackDir, isCrit);
        UpdateHealthBarUI();
    }

    public override void Die()
    {
        if (_isDead) return;

        // 1. Dropar Baú de Tesouro (com 1 a 3 upgrades)
        TreasureChest.Spawn(transform.position, chestUpgrades);

        // 2. Dropar gemas de XP vermelhas valiosas adicionais
        for (int i = 0; i < 3; i++)
        {
            Vector3 gemPos = transform.position + (Vector3)(Random.insideUnitCircle * 1.0f);
            ExperienceGem.Spawn(gemPos, 15);
        }

        // 3. Destruir barra de vida
        if (healthBarObj != null)
        {
            Destroy(healthBarObj);
        }

        // 4. Anúncio de vitória
        DamagePopup.Create(transform.position + Vector3.up * 1.5f, 0, true);

        base.Die();
    }

    private void CreateWorldHealthBar()
    {
        healthBarObj = new GameObject("BossHealthBar");

        // Fundo preto com borda
        GameObject bgObj = new GameObject("HP_BG");
        bgObj.transform.SetParent(healthBarObj.transform, false);
        SpriteRenderer bgSr = bgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite = CreateBarSprite(Color.black);
        bgSr.sortingOrder = 60;
        bgObj.transform.localScale = new Vector3(2.6f, 0.36f, 1f);

        // Barra de preenchimento vermelha
        GameObject fillObj = new GameObject("HP_Fill");
        fillObj.transform.SetParent(healthBarObj.transform, false);
        hpFillBar = fillObj.AddComponent<SpriteRenderer>();
        hpFillBar.sprite = CreateBarSprite(new Color(0.9f, 0.15f, 0.15f, 1f));
        hpFillBar.sortingOrder = 61;
        fillObj.transform.localScale = new Vector3(2.5f, 0.28f, 1f);

        // Texto do Chefe e HP
        GameObject textObj = new GameObject("HP_Text");
        textObj.transform.SetParent(healthBarObj.transform, false);
        hpTextTMP = textObj.AddComponent<TextMeshPro>();
        hpTextTMP.alignment = TextAlignmentOptions.Center;
        hpTextTMP.fontSize = 2.8f;
        hpTextTMP.fontStyle = FontStyles.Bold;
        hpTextTMP.color = Color.white;

        MeshRenderer mr = textObj.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 62;

        textObj.transform.localPosition = new Vector3(0, 0.35f, 0);

        UpdateHealthBarPosition();
    }

    private void UpdateHealthBarPosition()
    {
        if (healthBarObj != null)
        {
            float headOffset = Mathf.Max(1.5f, transform.localScale.y * 0.9f);
            healthBarObj.transform.position = transform.position + Vector3.up * headOffset;
        }
    }

    private void UpdateHealthBarUI()
    {
        if (hpFillBar != null && _originalHealth > 0)
        {
            float pct = Mathf.Clamp01((float)_health / _originalHealth);
            hpFillBar.transform.localScale = new Vector3(2.5f * pct, 0.28f, 1f);
            // Alinhamento à esquerda
            hpFillBar.transform.localPosition = new Vector3(-1.25f * (1f - pct), 0, 0);
        }

        if (hpTextTMP != null)
        {
            hpTextTMP.text = $"{bossTitle}  ({Mathf.Max(0, _health)}/{_originalHealth})";
        }
    }

    private static Sprite CreateBarSprite(Color c)
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }

    void OnDestroy()
    {
        if (healthBarObj != null)
        {
            Destroy(healthBarObj);
        }
    }
}
