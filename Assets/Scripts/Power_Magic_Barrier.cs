using System.Collections.Generic;
using UnityEngine;

public class Power_Magic_Barrier : MonoBehaviour
{
    [Header("Stats")]
    public int level = 0; // 0 = desativado
    public int orbCount = 1;
    public float damage = 18f;
    public float rotationSpeed = 160f;
    public float orbitRadius = 2.2f;

    private Transform playerTransform;
    private Player player;
    private List<GameObject> activeOrbs = new List<GameObject>();
    private static Sprite orbSprite;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            player = playerObj.GetComponent<Player>();
        }

        if (level > 0)
        {
            RebuildOrbs();
        }
    }

    void Update()
    {
        if (GlobalVariables._gameIsPaused || level <= 0) return;

        if (playerTransform != null)
        {
            transform.position = playerTransform.position;
        }

        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    public void UnlockOrUpgrade()
    {
        level++;
        if (level == 1)
        {
            orbCount = 1;
            damage = 18f;
            rotationSpeed = 160f;
        }
        else if (level == 2)
        {
            orbCount = 2;
            damage = 22f;
        }
        else if (level == 3)
        {
            orbCount = 3;
            rotationSpeed = 220f;
            damage = 26f;
        }
        else if (level == 4)
        {
            orbCount = 4;
            damage = 32f;
            rotationSpeed = 260f;
        }
        else
        {
            damage += 8f;
            rotationSpeed += 30f;
        }

        RebuildOrbs();
    }

    private void RebuildOrbs()
    {
        // Limpar orbes antigas
        for (int i = 0; i < activeOrbs.Count; i++)
        {
            if (activeOrbs[i] != null) Destroy(activeOrbs[i]);
        }
        activeOrbs.Clear();

        if (orbSprite == null)
        {
            orbSprite = CreateCircleSprite();
        }

        for (int i = 0; i < orbCount; i++)
        {
            float angle = (360f / orbCount) * i * Mathf.Deg2Rad;
            Vector3 localPos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * orbitRadius;

            GameObject orb = new GameObject("MagicOrb_" + i);
            orb.transform.SetParent(transform);
            orb.transform.localPosition = localPos;
            orb.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            SpriteRenderer sr = orb.AddComponent<SpriteRenderer>();
            sr.sprite = orbSprite;
            sr.color = new Color(0.4f, 0.8f, 1f, 0.85f);
            sr.sortingOrder = 4;

            CircleCollider2D col = orb.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;

            MagicOrbDamage orbDmg = orb.AddComponent<MagicOrbDamage>();
            orbDmg.barrier = this;

            activeOrbs.Add(orb);
        }
    }

    public void OnOrbHitEnemy(Enemy enemy, Vector3 orbPos)
    {
        if (enemy != null && !enemy.IsDead)
        {
            float finalDmg = damage * (player != null ? player.damageMultiplier : 1f);
            Vector2 knockback = (enemy.transform.position - transform.position).normalized;
            enemy.TakeDamage((int)finalDmg, knockback);
        }
    }

    private static Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = new Color(0, 0, 0, 0);
        Color lightCyan = new Color(0.6f, 0.9f, 1f, 1f);
        Color deepCyan = new Color(0.2f, 0.6f, 1f, 0.9f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                if (dist <= radius)
                {
                    float factor = dist / radius;
                    tex.SetPixel(x, y, Color.Lerp(lightCyan, deepCyan, factor));
                }
                else
                {
                    tex.SetPixel(x, y, clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }
}

public class MagicOrbDamage : MonoBehaviour
{
    public Power_Magic_Barrier barrier;
    private Dictionary<Enemy, float> hitCooldowns = new Dictionary<Enemy, float>();

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Enemy>(out Enemy enemy))
        {
            if (!hitCooldowns.ContainsKey(enemy) || Time.time >= hitCooldowns[enemy] + 0.35f)
            {
                hitCooldowns[enemy] = Time.time;
                if (barrier != null)
                {
                    barrier.OnOrbHitEnemy(enemy, transform.position);
                }
            }
        }
    }
}
