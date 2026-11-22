using System.Collections.Generic;
using UnityEngine;

public class Weapon_ThrowingKnives : MonoBehaviour
{
    [Header("Stats")]
    public int level = 0; // 0 = desativado
    public float baseInterval = 1.2f;
    public float knifeDamage = 16f;
    public float knifeSpeed = 16f;
    public int knifeCount = 1;
    public int pierceCount = 1;

    private float timer = 0f;
    private Player player;
    private static Sprite cachedKnifeSprite;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<Player>();
        }
    }

    void Update()
    {
        if (GlobalVariables._gameIsPaused || level <= 0 || player == null) return;

        timer += Time.deltaTime;
        float actualInterval = Mathf.Max(0.2f, baseInterval * player.cooldownMultiplier);

        if (timer >= actualInterval)
        {
            timer = 0f;
            FireKnives();
        }
    }

    public void UnlockOrUpgrade()
    {
        level++;
        if (level == 1)
        {
            knifeCount = 1;
            knifeDamage = 16f;
            baseInterval = 1.2f;
            pierceCount = 1;
        }
        else if (level == 2)
        {
            knifeCount = 2;
            knifeDamage = 20f;
            baseInterval = 1.1f;
            pierceCount = 1;
        }
        else if (level == 3)
        {
            knifeCount = 3;
            knifeDamage = 26f;
            baseInterval = 0.95f;
            pierceCount = 2;
        }
        else if (level == 4)
        {
            knifeCount = 4;
            knifeDamage = 32f;
            knifeSpeed = 20f;
            baseInterval = 0.8f;
            pierceCount = 2;
        }
        else
        {
            knifeCount += 1;
            knifeDamage += 8f;
            pierceCount += 1;
            baseInterval = Mathf.Max(0.4f, baseInterval - 0.05f);
        }
    }

    private void FireKnives()
    {
        SoundManager.PlayShoot();
        Vector2 dir = player.LastFacingDirection;
        if (dir.sqrMagnitude < 0.01f) dir = Vector2.right;

        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        int total = knifeCount + player.extraProjectiles;

        if (cachedKnifeSprite == null)
        {
            cachedKnifeSprite = CreateKnifeSprite();
        }

        if (total == 1)
        {
            SpawnSingleKnife(baseAngle);
        }
        else
        {
            float spread = 9f; // Ângulo de abertura em leque estreito
            float startAngle = baseAngle - (spread * (total - 1) / 2f);

            for (int i = 0; i < total; i++)
            {
                SpawnSingleKnife(startAngle + (i * spread));
            }
        }
    }

    private void SpawnSingleKnife(float angleDeg)
    {
        GameObject knife = new GameObject("ThrowingKnife");
        knife.transform.position = player.transform.position;
        knife.transform.rotation = Quaternion.Euler(0, 0, angleDeg);

        SpriteRenderer sr = knife.AddComponent<SpriteRenderer>();
        sr.sprite = cachedKnifeSprite;
        sr.sortingOrder = 5;

        CircleCollider2D col = knife.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        ThrowingKnifeProjectile proj = knife.AddComponent<ThrowingKnifeProjectile>();
        proj.Setup(player, knifeDamage, knifeSpeed, pierceCount);
    }

    private static Sprite CreateKnifeSprite()
    {
        int w = 24;
        int h = 10;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                tex.SetPixel(x, y, clear);
            }
        }

        int centerY = h / 2;

        // Desenhar adaga afiada apontando para a direita (+X)
        for (int x = 0; x < w; x++)
        {
            if (x < 6)
            {
                // Cabo / Punho
                for (int y = centerY - 1; y <= centerY + 1; y++)
                {
                    tex.SetPixel(x, y, new Color(0.45f, 0.25f, 0.1f, 1f));
                }
            }
            else if (x < 8)
            {
                // Guarda dourada
                for (int y = centerY - 3; y <= centerY + 3; y++)
                {
                    tex.SetPixel(x, y, new Color(0.95f, 0.75f, 0.2f, 1f));
                }
            }
            else
            {
                // Lâmina prateada afunilando até a ponta (x = 23)
                int maxDy = Mathf.Max(0, Mathf.RoundToInt((23 - x) * 0.25f + 1f));
                for (int y = centerY - maxDy; y <= centerY + maxDy; y++)
                {
                    if (y == centerY)
                        tex.SetPixel(x, y, new Color(0.95f, 0.98f, 1f, 1f)); // Fio de luz
                    else
                        tex.SetPixel(x, y, new Color(0.7f, 0.8f, 0.9f, 1f)); // Aço prateado
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 24f);
    }
}

public class ThrowingKnifeProjectile : MonoBehaviour
{
    private Player player;
    private float damage;
    private float speed;
    private int remainingPierces;
    private float lifeTime = 2.2f;
    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    public void Setup(Player p, float dmg, float spd, int pierces)
    {
        player = p;
        damage = dmg;
        speed = spd;
        remainingPierces = pierces;
    }

    void Update()
    {
        if (GlobalVariables._gameIsPaused) return;

        transform.position += transform.right * speed * Time.deltaTime;
        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Enemy>(out Enemy enemy))
        {
            if (enemy == null || enemy.IsDead || hitEnemies.Contains(enemy)) return;

            hitEnemies.Add(enemy);
            bool isCrit = false;
            int finalDmg = player != null ? player.CalculateDamage(damage, out isCrit) : (int)damage;
            enemy.TakeDamage(finalDmg, transform.right, isCrit);

            remainingPierces--;
            if (remainingPierces <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}

