using System.Collections.Generic;
using UnityEngine;

public class Weapon_Aura : MonoBehaviour
{
    [Header("Stats")]
    public int level = 0; // 0 = desativado
    public float auraRadius = 3.0f;
    public float damage = 12f;
    public float pulseInterval = 1.0f;
    public LayerMask enemyLayer;

    private Transform playerTransform;
    private Player player;
    private float timer = 0f;
    private GameObject visualRing;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            player = playerObj.GetComponent<Player>();
        }

        if (enemyLayer.value == 0)
        {
            enemyLayer = 1 << 6; // Layer 6 = Enemy
        }

        if (level > 0)
        {
            CreateVisualRing();
        }
    }

    void Update()
    {
        if (GlobalVariables._gameIsPaused || level <= 0) return;

        if (playerTransform != null)
        {
            transform.position = playerTransform.position;
        }

        timer += Time.deltaTime;
        float actualInterval = Mathf.Max(0.3f, pulseInterval * (player != null ? player.cooldownMultiplier : 1f));

        if (timer >= actualInterval)
        {
            timer = 0f;
            PulseDamage();
        }
    }

    public void UnlockOrUpgrade()
    {
        level++;
        if (level == 1)
        {
            auraRadius = 3.0f;
            damage = 12f;
            pulseInterval = 1.1f;
        }
        else if (level == 2)
        {
            auraRadius = 3.8f;
            damage = 16f;
        }
        else if (level == 3)
        {
            auraRadius = 4.5f;
            damage = 22f;
            pulseInterval = 0.9f;
        }
        else if (level == 4)
        {
            auraRadius = 5.2f;
            damage = 30f;
            pulseInterval = 0.75f;
        }
        else
        {
            damage += 8f;
            auraRadius += 0.5f;
        }

        CreateVisualRing();
    }

    private void PulseDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, auraRadius, enemyLayer);

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null) continue;
            Enemy enemy = enemies[i].GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                bool isCrit = false;
                int finalDamage = player != null ? player.CalculateDamage(damage, out isCrit) : (int)damage;
                Vector2 knockback = (enemy.transform.position - transform.position).normalized;
                enemy.TakeDamage(finalDamage, knockback, isCrit);
            }
        }
    }

    private void CreateVisualRing()
    {
        if (visualRing == null)
        {
            visualRing = new GameObject("AuraVisualRing");
            visualRing.transform.SetParent(transform);
            visualRing.transform.localPosition = Vector3.zero;

            SpriteRenderer sr = visualRing.AddComponent<SpriteRenderer>();
            sr.sprite = CreateRingSprite();
            sr.color = new Color(1f, 0.9f, 0.2f, 0.25f); // Amarelo sagrado translúcido
            sr.sortingOrder = 1;
        }

        visualRing.transform.localScale = new Vector3(auraRadius * 2f, auraRadius * 2f, 1f);
    }

    private static Sprite CreateRingSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = new Color(0, 0, 0, 0);
        Color auraColor = new Color(1f, 0.9f, 0.3f, 0.8f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                if (dist <= radius && dist >= radius - 3.5f)
                {
                    tex.SetPixel(x, y, auraColor);
                }
                else if (dist < radius - 3.5f)
                {
                    float insideAlpha = Mathf.Lerp(0.2f, 0.05f, (radius - 3.5f - dist) / radius);
                    tex.SetPixel(x, y, new Color(1f, 0.95f, 0.4f, insideAlpha));
                }
                else
                {
                    tex.SetPixel(x, y, clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }
}
