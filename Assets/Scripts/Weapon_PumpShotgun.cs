using System.Collections.Generic;
using UnityEngine;

public class Weapon_PumpShotgun : MonoBehaviour
{
    [Header("Stats")]
    public int level = 0; // 0 = desativado
    public float baseInterval = 1.35f;
    public float pelletDamage = 16f;
    public int pelletCount = 5;
    public float spreadAngle = 32f;
    public float pelletSpeed = 20f;
    public float knockbackForce = 3.8f;
    public int pierceCount = 1;
    public float detectionRadius = 11f;

    private float timer = 0f;
    private Player player;
    private LayerMask enemyLayer;
    private static Collider2D[] shotgunAimBuffer = new Collider2D[64];
    private static Sprite cachedPelletSprite;

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
        float actualInterval = Mathf.Max(0.35f, baseInterval * player.cooldownMultiplier);

        if (timer >= actualInterval)
        {
            timer = 0f;
            FirePumpShotgun();
        }
    }

    public void UnlockOrUpgrade()
    {
        level++;
        if (level == 1)
        {
            pelletDamage = 16f;
            pelletCount = 5;
            baseInterval = 1.35f;
            spreadAngle = 32f;
            pierceCount = 1;
            knockbackForce = 3.8f;
        }
        else if (level == 2)
        {
            pelletCount = 7;
            pelletDamage = 20f;
            spreadAngle = 38f;
            baseInterval = 1.25f;
        }
        else if (level == 3)
        {
            // MELHORIA: CHUMBO PESADO & KNOCKBACK REFORÇADO
            pierceCount = 2; // Perfura inimigos!
            pelletDamage = 24f;
            knockbackForce = 5.5f;
        }
        else if (level == 4)
        {
            // MELHORIA: AÇÃO RÁPIDA DA BOMBA (FAST PUMP)
            baseInterval = 0.98f;
            pelletCount = 9;
            pelletDamage = 28f;
        }
        else
        {
            // Nível 5+: SUPER SHOTGUN DEVASTADORA
            pelletCount += 3; // 12 balins!
            pierceCount = 3;
            pelletDamage += 8f;
            knockbackForce = 6.8f;
            baseInterval = Mathf.Max(0.55f, baseInterval - 0.1f);
        }
    }

    private void FirePumpShotgun()
    {
        SoundManager.PlayShotgun();

        Vector2 fireDir = player.LastFacingDirection;
        if (fireDir.sqrMagnitude < 0.01f) fireDir = Vector2.right;

        // Mira prioritária no inimigo mais próximo
        Transform target = GetClosestEnemyInRadius(detectionRadius);
        if (target != null)
        {
            Vector2 toTarget = ((Vector2)target.position - (Vector2)player.transform.position).normalized;
            if (toTarget.sqrMagnitude > 0.01f)
            {
                fireDir = toTarget;
            }
        }

        float centerAngle = Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg;
        int totalPellets = pelletCount + (player != null ? player.extraProjectiles * 2 : 0);

        if (cachedPelletSprite == null) cachedPelletSprite = CreatePelletSprite();

        // Disparo em cone com dispersão angular
        float startAngle = centerAngle - (spreadAngle * 0.5f);
        float angleStep = spreadAngle / Mathf.Max(1, totalPellets - 1);

        for (int i = 0; i < totalPellets; i++)
        {
            float jitter = Random.Range(-2.5f, 2.5f);
            float angle = startAngle + (i * angleStep) + jitter;
            SpawnPellet(angle);
        }
    }

    private void SpawnPellet(float angleDeg)
    {
        Vector2 dir = new Vector2(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));

        GameObject pelletObj = new GameObject("ShotgunPellet");
        pelletObj.transform.position = player.transform.position + (Vector3)(dir * 0.35f);
        pelletObj.transform.rotation = Quaternion.Euler(0, 0, angleDeg);

        SpriteRenderer sr = pelletObj.AddComponent<SpriteRenderer>();
        sr.sprite = cachedPelletSprite;
        sr.sortingOrder = 9;
        sr.color = new Color(1f, 0.7f, 0.2f, 1f);

        CircleCollider2D col = pelletObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.25f;

        Rigidbody2D rb = pelletObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        ShotgunPellet proj = pelletObj.AddComponent<ShotgunPellet>();
        float speedVariation = pelletSpeed * Random.Range(0.88f, 1.12f);
        proj.Setup(player, pelletDamage, speedVariation, knockbackForce, pierceCount);
    }

    private Transform GetClosestEnemyInRadius(float searchRadius)
    {
        int count = Physics2D.OverlapCircleNonAlloc(player.transform.position, searchRadius, shotgunAimBuffer, enemyLayer);
        Transform closest = null;
        float minSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (shotgunAimBuffer[i] == null) continue;
            Enemy enemy = shotgunAimBuffer[i].GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                float sqrDist = ((Vector2)enemy.transform.position - (Vector2)player.transform.position).sqrMagnitude;
                if (sqrDist < minSqr)
                {
                    minSqr = sqrDist;
                    closest = enemy.transform;
                }
            }
        }

        return closest;
    }

    private static Sprite CreatePelletSprite()
    {
        int size = 12;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++) tex.SetPixel(x, y, clear);
        }

        float center = size / 2f;
        float radius = 4f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
                if (dist <= radius)
                {
                    float factor = 1f - (dist / radius);
                    Color c = Color.Lerp(new Color(1f, 0.4f, 0.05f, 0.95f), Color.white, factor * 0.8f);
                    tex.SetPixel(x, y, c);
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 24f);
    }
}

public class ShotgunPellet : MonoBehaviour
{
    private Player player;
    private float damage;
    private float speed;
    private float knockback;
    private int remainingPierces;
    private float lifeTime = 0.55f; // Alcance moderado com decaimento
    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    public void Setup(Player p, float dmg, float spd, float kb, int pierce)
    {
        player = p;
        damage = dmg;
        speed = spd;
        knockback = kb;
        remainingPierces = pierce;
    }

    void Update()
    {
        if (GlobalVariables._gameIsPaused) return;

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += transform.right * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Enemy>(out Enemy enemy))
        {
            if (enemy.IsDead || hitEnemies.Contains(enemy)) return;

            hitEnemies.Add(enemy);
            bool isCrit = false;
            int finalDmg = player != null ? player.CalculateDamage(damage, out isCrit) : (int)damage;
            Vector2 kbDir = transform.right * knockback;
            enemy.TakeDamage(finalDmg, kbDir, isCrit);

            remainingPierces--;
            if (remainingPierces <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
