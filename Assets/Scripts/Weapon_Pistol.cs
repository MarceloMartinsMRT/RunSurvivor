using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Pistol : MonoBehaviour
{
    [Header("Stats")]
    public int level = 0; // 0 = desativado
    public float baseInterval = 0.52f;
    public float bulletDamage = 22f;
    public float bulletSpeed = 24f;
    public int burstCount = 1; // Tiros por rajada (Akimbo / Dual Tap)
    public float burstDelay = 0.08f;
    public int pierceCount = 1;
    public float detectionRadius = 14f;

    private float timer = 0f;
    private Player player;
    private LayerMask enemyLayer;
    private static Collider2D[] pistolAimBuffer = new Collider2D[64];
    private static Sprite cachedBulletSprite;

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
        float actualInterval = Mathf.Max(0.18f, baseInterval * player.cooldownMultiplier);

        if (timer >= actualInterval)
        {
            timer = 0f;
            StartCoroutine(ExecutePistolBurst());
        }
    }

    public void UnlockOrUpgrade()
    {
        level++;
        if (level == 1)
        {
            bulletDamage = 22f;
            baseInterval = 0.52f;
            burstCount = 1;
            pierceCount = 1;
            bulletSpeed = 24f;
        }
        else if (level == 2)
        {
            bulletDamage = 28f;
            baseInterval = 0.42f;
        }
        else if (level == 3)
        {
            // MELHORIA: TIRO DUPLO (Dual Tap / Akimbo)
            burstCount = 2;
            bulletDamage = 32f;
            baseInterval = 0.38f;
        }
        else if (level == 4)
        {
            // MELHORIA: BALAS PERFURANTES
            pierceCount = 3;
            bulletDamage = 38f;
            bulletSpeed = 28f;
        }
        else
        {
            // Nível 5+: MAGNUM FRENÉTICA
            burstCount = 3; // Rajada tripla!
            pierceCount = 4;
            bulletDamage += 12f;
            baseInterval = Mathf.Max(0.25f, baseInterval - 0.05f);
        }
    }

    private IEnumerator ExecutePistolBurst()
    {
        int shotsToFire = burstCount;

        for (int i = 0; i < shotsToFire; i++)
        {
            FireSingleBullet();

            if (i < shotsToFire - 1)
            {
                yield return new WaitForSeconds(burstDelay);
            }
        }
    }

    private void FireSingleBullet()
    {
        SoundManager.PlayPistol();

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

        float angleDeg = Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg;

        if (cachedBulletSprite == null) cachedBulletSprite = CreateBulletSprite();

        // Projétil
        GameObject bulletObj = new GameObject("PistolBullet");
        bulletObj.transform.position = player.transform.position + (Vector3)(fireDir * 0.4f);
        bulletObj.transform.rotation = Quaternion.Euler(0, 0, angleDeg);

        SpriteRenderer sr = bulletObj.AddComponent<SpriteRenderer>();
        sr.sprite = cachedBulletSprite;
        sr.sortingOrder = 9;
        sr.color = new Color(1f, 0.95f, 0.4f, 1f);

        BoxCollider2D col = bulletObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.7f, 0.25f);

        Rigidbody2D rb = bulletObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        PistolBullet proj = bulletObj.AddComponent<PistolBullet>();
        proj.Setup(player, bulletDamage, bulletSpeed, pierceCount);
    }

    private Transform GetClosestEnemyInRadius(float searchRadius)
    {
        int count = Physics2D.OverlapCircleNonAlloc(player.transform.position, searchRadius, pistolAimBuffer, enemyLayer);
        Transform closest = null;
        float minSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (pistolAimBuffer[i] == null) continue;
            Enemy enemy = pistolAimBuffer[i].GetComponent<Enemy>();
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

    private static Sprite CreateBulletSprite()
    {
        int w = 24;
        int h = 10;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++) tex.SetPixel(x, y, clear);
        }

        int centerY = h / 2;

        // Desenha projétil perfurante aerodinâmico
        for (int x = 2; x < w - 2; x++)
        {
            float normX = (float)x / w;
            int maxDy = Mathf.RoundToInt(Mathf.Lerp(3f, 1f, normX));

            for (int y = 0; y < h; y++)
            {
                int dy = Mathf.Abs(y - centerY);
                if (dy <= maxDy)
                {
                    Color c = Color.Lerp(new Color(1f, 0.5f, 0.1f, 0.9f), Color.white, normX);
                    tex.SetPixel(x, y, c);
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 24f);
    }
}

public class PistolBullet : MonoBehaviour
{
    private Player player;
    private float damage;
    private float speed;
    private int remainingPierces;
    private float lifeTime = 1.5f;
    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    public void Setup(Player p, float dmg, float spd, int pierce)
    {
        player = p;
        damage = dmg;
        speed = spd;
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
            Vector2 knockback = transform.right * 1.6f;
            enemy.TakeDamage(finalDmg, knockback, isCrit);

            remainingPierces--;
            if (remainingPierces <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
