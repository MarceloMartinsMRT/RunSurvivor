using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_MeteorRain : MonoBehaviour
{
    [Header("Stats")]
    public int level = 0; // 0 = desativado
    public float baseInterval = 2.2f;
    public float meteorDamage = 35f;
    public float blastRadius = 1.8f;
    public int meteorCount = 1;

    private float timer = 0f;
    private Player player;
    private LayerMask enemyLayer;

    private static Sprite warningSprite;
    private static Sprite blastSprite;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<Player>();
        }

        enemyLayer = 1 << 6; // Layer 6 = Enemy
    }

    void Update()
    {
        if (GlobalVariables._gameIsPaused || level <= 0 || player == null) return;

        timer += Time.deltaTime;
        float actualInterval = Mathf.Max(0.5f, baseInterval * player.cooldownMultiplier);

        if (timer >= actualInterval)
        {
            timer = 0f;
            StartCoroutine(TriggerMeteorShower());
        }
    }

    public void UnlockOrUpgrade()
    {
        level++;
        if (level == 1)
        {
            meteorCount = 1;
            meteorDamage = 35f;
            blastRadius = 1.8f;
            baseInterval = 2.2f;
        }
        else if (level == 2)
        {
            meteorCount = 2;
            meteorDamage = 42f;
        }
        else if (level == 3)
        {
            meteorCount = 3;
            meteorDamage = 52f;
            blastRadius = 2.4f;
            baseInterval = 1.9f;
        }
        else if (level == 4)
        {
            meteorCount = 4;
            meteorDamage = 68f;
            baseInterval = 1.6f;
        }
        else
        {
            meteorCount += 1;
            meteorDamage += 16f;
            blastRadius += 0.35f;
            baseInterval = Mathf.Max(0.8f, baseInterval - 0.1f);
        }
    }

    private IEnumerator TriggerMeteorShower()
    {
        int total = meteorCount + player.extraProjectiles;

        // Procura inimigos num raio de 9 unidades
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(player.transform.position, 9f, enemyLayer);
        List<Vector3> targetPositions = new List<Vector3>();

        for (int i = 0; i < potentialTargets.Length; i++)
        {
            if (potentialTargets[i] != null)
            {
                Enemy e = potentialTargets[i].GetComponent<Enemy>();
                if (e != null && !e.IsDead)
                {
                    targetPositions.Add(potentialTargets[i].transform.position);
                }
            }
        }

        // Embaralha alvos
        for (int i = targetPositions.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            Vector3 temp = targetPositions[i];
            targetPositions[i] = targetPositions[r];
            targetPositions[r] = temp;
        }

        for (int i = 0; i < total; i++)
        {
            Vector3 impactPos;
            if (i < targetPositions.Count)
            {
                impactPos = targetPositions[i] + (Vector3)(Random.insideUnitCircle * 0.4f);
            }
            else
            {
                impactPos = player.transform.position + (Vector3)(Random.insideUnitCircle * 5.5f);
            }

            StartCoroutine(SpawnMeteorAt(impactPos));

            // Pequeno atraso em cascata entre meteoros para efeito dramático
            if (i < total - 1)
            {
                yield return new WaitForSeconds(0.12f);
            }
        }
    }

    private IEnumerator SpawnMeteorAt(Vector3 position)
    {
        if (warningSprite == null) warningSprite = CreateWarningCircleSprite();
        if (blastSprite == null) blastSprite = CreateBlastSprite();

        // 1. Indicador de Mira / Runas no chão
        GameObject warningObj = new GameObject("MeteorWarning");
        warningObj.transform.position = position;
        warningObj.transform.localScale = Vector3.one * (blastRadius * 2f);

        SpriteRenderer warnSr = warningObj.AddComponent<SpriteRenderer>();
        warnSr.sprite = warningSprite;
        warnSr.sortingOrder = 2;
        warnSr.color = new Color(1f, 0.35f, 0.1f, 0.55f);

        float warnTimer = 0.22f;
        while (warnTimer > 0f)
        {
            if (!GlobalVariables._gameIsPaused)
            {
                warnTimer -= Time.deltaTime;
                float scale = (blastRadius * 2f) * (0.8f + (0.22f - warnTimer) * 0.9f);
                warningObj.transform.localScale = new Vector3(scale, scale, 1f);
            }
            yield return null;
        }
        Destroy(warningObj);

        // 2. Queda e Explosão Celestial
        GameObject blastObj = new GameObject("MeteorBlast");
        blastObj.transform.position = position;
        blastObj.transform.localScale = Vector3.one * (blastRadius * 2.2f);

        SpriteRenderer blastSr = blastObj.AddComponent<SpriteRenderer>();
        blastSr.sprite = blastSprite;
        blastSr.sortingOrder = 7;
        blastSr.color = Color.white;

        // Dano em Área
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, blastRadius, enemyLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;
            Enemy enemy = hits[i].GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                bool isCrit = false;
                int dmg = player != null ? player.CalculateDamage(meteorDamage, out isCrit) : (int)meteorDamage;
                Vector2 knockback = ((Vector3)hits[i].transform.position - position).normalized;
                if (knockback.sqrMagnitude < 0.01f) knockback = Random.insideUnitCircle.normalized;

                enemy.TakeDamage(dmg, knockback * 1.5f, isCrit);
            }
        }

        // Animação de expansão e fade out da explosão
        float fadeTime = 0.22f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            if (!GlobalVariables._gameIsPaused)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeTime;
                blastObj.transform.localScale = Vector3.one * (blastRadius * 2.2f * (1f + t * 0.35f));
                Color c = Color.Lerp(Color.white, new Color(1f, 0.25f, 0f, 0f), t);
                blastSr.color = c;
            }
            yield return null;
        }
        Destroy(blastObj);
    }

    private static Sprite CreateWarningCircleSprite()
    {
        int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = new Color(0, 0, 0, 0);
        Color redEdge = new Color(1f, 0.2f, 0.1f, 0.85f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                if (dist <= radius && dist >= radius - 3.5f)
                {
                    tex.SetPixel(x, y, redEdge);
                }
                else if (dist < radius - 3.5f)
                {
                    tex.SetPixel(x, y, new Color(1f, 0.4f, 0.1f, 0.2f));
                }
                else
                {
                    tex.SetPixel(x, y, clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 48f);
    }

    private static Sprite CreateBlastSprite()
    {
        int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = new Color(0, 0, 0, 0);
        Color core = new Color(1f, 1f, 0.7f, 1f);       // Núcleo branco-amarelado
        Color mid = new Color(1f, 0.6f, 0.1f, 0.9f);     // Laranja incandescente
        Color edge = new Color(0.9f, 0.15f, 0.05f, 0.8f); // Vermelho escuro
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                if (dist <= radius)
                {
                    float factor = dist / radius;
                    Color c = factor < 0.4f ? Color.Lerp(core, mid, factor / 0.4f) : Color.Lerp(mid, edge, (factor - 0.4f) / 0.6f);
                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 48f);
    }
}
