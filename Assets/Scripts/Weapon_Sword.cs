using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Sword : MonoBehaviour
{
    [Header("Stats")]
    public int level = 0; // 0 = desativado
    public float baseInterval = 1.05f;
    public float swordDamage = 32f;
    public float slashRadius = 3.2f;
    public float arcAngle = 105f; // Ângulo em graus do arco de dano
    public bool isCircularSlash = false; // Nível 3: Dano circular 360°
    public bool launchesSwordWave = false; // Nível 4: Lança corte perfurante
    public int swordWaveCount = 1;
    public float swordWaveDamage = 28f;
    public float swordWaveSpeed = 15f;
    public int swordWavePierce = 3;

    private float timer = 0f;
    private Player player;
    private LayerMask enemyLayer;
    private static Collider2D[] swordHitBuffer = new Collider2D[64];
    private static Sprite cachedSlashSprite;
    private static Sprite cachedCircleSprite;
    private static Sprite cachedWaveSprite;

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
        float actualInterval = Mathf.Max(0.25f, baseInterval * player.cooldownMultiplier);

        if (timer >= actualInterval)
        {
            timer = 0f;
            ExecuteSwordAttack();
        }
    }

    public void UnlockOrUpgrade()
    {
        level++;
        if (level == 1)
        {
            swordDamage = 32f;
            slashRadius = 3.2f;
            arcAngle = 105f;
            baseInterval = 1.05f;
            isCircularSlash = false;
            launchesSwordWave = false;
            swordWaveCount = 1;
        }
        else if (level == 2)
        {
            swordDamage = 42f;
            slashRadius = 3.8f;
            arcAngle = 140f;
            baseInterval = 0.95f;
        }
        else if (level == 3)
        {
            // MELHORIA: DANO CIRCULAR 360° (Giro de Espada / Whirlwind)
            isCircularSlash = true;
            swordDamage = 54f;
            slashRadius = 4.2f;
            baseInterval = 0.88f;
        }
        else if (level == 4)
        {
            // MELHORIA: LANÇAMENTO DE CORTE (Onda de Espada / Vento Cortante)
            launchesSwordWave = true;
            swordWaveCount = 1;
            swordWaveDamage = 35f;
            swordDamage = 62f;
            baseInterval = 0.80f;
        }
        else
        {
            // Nível 5+: MESTRIA DA LÂMINA
            swordDamage += 16f;
            swordWaveCount = 3; // Dispara 3 ondas cortantes em leque!
            swordWaveDamage += 10f;
            slashRadius += 0.4f;
            baseInterval = Mathf.Max(0.45f, baseInterval - 0.08f);
        }
    }

    private void ExecuteSwordAttack()
    {
        SoundManager.PlaySwordSlash();

        // Determina a direção principal do golpe
        Vector2 attackDir = player.LastFacingDirection;
        if (attackDir.sqrMagnitude < 0.01f) attackDir = Vector2.right;

        // Tenta mirar no inimigo mais próximo se houver algum no alcance
        Transform closestEnemy = GetClosestEnemyInRadius(slashRadius + 2f);
        if (closestEnemy != null)
        {
            Vector2 toEnemy = ((Vector2)closestEnemy.position - (Vector2)player.transform.position).normalized;
            if (toEnemy.sqrMagnitude > 0.01f)
            {
                attackDir = toEnemy;
            }
        }

        float angleDeg = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;

        // 1. Aplica o Dano e exibe o Visual da Espada (Arco ou Circular)
        if (isCircularSlash)
        {
            PerformCircularSlash();
        }
        else
        {
            PerformArcSlash(attackDir, angleDeg);
        }

        // 2. Se a melhoria de Lançamento de Corte estiver desbloqueada, dispara as ondas cortantes!
        if (launchesSwordWave)
        {
            LaunchSwordWaves(attackDir, angleDeg);
        }
    }

    private void PerformArcSlash(Vector2 attackDir, float angleDeg)
    {
        // Visual de corte em arco
        if (cachedSlashSprite == null) cachedSlashSprite = CreateSlashArcSprite();

        GameObject slashObj = new GameObject("SwordArcSlash");
        slashObj.transform.position = player.transform.position + (Vector3)(attackDir * (slashRadius * 0.45f));
        slashObj.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
        slashObj.transform.localScale = new Vector3(slashRadius * 0.9f, slashRadius * 0.7f, 1f);

        SpriteRenderer sr = slashObj.AddComponent<SpriteRenderer>();
        sr.sprite = cachedSlashSprite;
        sr.sortingOrder = 9;
        sr.color = new Color(0.6f, 0.9f, 1f, 0.95f);

        // Detecção de Dano em Arco sem GC Alloc
        int hitCount = Physics2D.OverlapCircleNonAlloc(player.transform.position, slashRadius, swordHitBuffer, enemyLayer);
        HashSet<Enemy> hitEnemies = new HashSet<Enemy>();
        float halfArc = arcAngle * 0.5f;

        for (int i = 0; i < hitCount; i++)
        {
            if (swordHitBuffer[i] == null) continue;
            Enemy enemy = swordHitBuffer[i].GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead && !hitEnemies.Contains(enemy))
            {
                Vector2 toEnemy = ((Vector2)enemy.transform.position - (Vector2)player.transform.position).normalized;
                float angleBetween = Vector2.Angle(attackDir, toEnemy);

                if (angleBetween <= halfArc)
                {
                    hitEnemies.Add(enemy);
                    bool isCrit = false;
                    int dmg = player != null ? player.CalculateDamage(swordDamage, out isCrit) : (int)swordDamage;
                    Vector2 knockback = toEnemy * 2.2f;
                    enemy.TakeDamage(dmg, knockback, isCrit);
                }
            }
        }

        StartCoroutine(AnimateArcSlash(slashObj, sr));
    }

    private void PerformCircularSlash()
    {
        // Visual do Giro 360° da Espada
        if (cachedCircleSprite == null) cachedCircleSprite = CreateCircleSlashSprite();

        GameObject circleObj = new GameObject("SwordWhirlwindSlash");
        circleObj.transform.position = player.transform.position;
        circleObj.transform.localScale = Vector3.one * (slashRadius * 0.4f);

        SpriteRenderer sr = circleObj.AddComponent<SpriteRenderer>();
        sr.sprite = cachedCircleSprite;
        sr.sortingOrder = 9;
        sr.color = new Color(0.4f, 0.85f, 1f, 0.95f);

        // Dano em todos os inimigos 360° ao redor do jogador (Zero GC Alloc)
        int hitCount = Physics2D.OverlapCircleNonAlloc(player.transform.position, slashRadius, swordHitBuffer, enemyLayer);
        HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

        for (int i = 0; i < hitCount; i++)
        {
            if (swordHitBuffer[i] == null) continue;
            Enemy enemy = swordHitBuffer[i].GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead && !hitEnemies.Contains(enemy))
            {
                hitEnemies.Add(enemy);
                bool isCrit = false;
                int dmg = player != null ? player.CalculateDamage(swordDamage, out isCrit) : (int)swordDamage;
                Vector2 pushDir = ((Vector2)enemy.transform.position - (Vector2)player.transform.position).normalized;
                if (pushDir.sqrMagnitude < 0.01f) pushDir = Vector2.up;
                Vector2 knockback = pushDir * 3.0f;
                enemy.TakeDamage(dmg, knockback, isCrit);
            }
        }

        StartCoroutine(AnimateCircularSlash(circleObj, sr));
    }

    private void LaunchSwordWaves(Vector2 forwardDir, float centerAngle)
    {
        if (cachedWaveSprite == null) cachedWaveSprite = CreateSwordWaveSprite();

        int totalWaves = swordWaveCount + player.extraProjectiles;

        if (totalWaves == 1)
        {
            SpawnSingleSwordWave(centerAngle);
        }
        else
        {
            float totalSpread = Mathf.Min(60f, (totalWaves - 1) * 18f);
            float startAngle = centerAngle - (totalSpread / 2f);
            float step = totalSpread / (totalWaves - 1);

            for (int i = 0; i < totalWaves; i++)
            {
                float angle = startAngle + (i * step);
                SpawnSingleSwordWave(angle);
            }
        }
    }

    private void SpawnSingleSwordWave(float angleDeg)
    {
        GameObject waveObj = new GameObject("SwordWave");
        waveObj.transform.position = player.transform.position;
        waveObj.transform.rotation = Quaternion.Euler(0, 0, angleDeg);

        SpriteRenderer sr = waveObj.AddComponent<SpriteRenderer>();
        sr.sprite = cachedWaveSprite;
        sr.sortingOrder = 9;
        sr.color = new Color(0.7f, 1f, 1f, 0.95f);

        BoxCollider2D col = waveObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.2f, 0.6f);

        Rigidbody2D rb = waveObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        SwordWaveProjectile proj = waveObj.AddComponent<SwordWaveProjectile>();
        proj.Setup(player, swordWaveDamage, swordWaveSpeed, swordWavePierce);
    }

    private IEnumerator AnimateArcSlash(GameObject slashObj, SpriteRenderer sr)
    {
        float duration = 0.16f;
        float elapsed = 0f;
        Vector3 initialScale = slashObj.transform.localScale;

        while (elapsed < duration)
        {
            if (!GlobalVariables._gameIsPaused)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (slashObj != null)
                {
                    slashObj.transform.localScale = initialScale * (1f + t * 0.35f);
                    if (sr != null)
                    {
                        sr.color = Color.Lerp(new Color(0.7f, 0.95f, 1f, 1f), new Color(0.2f, 0.6f, 1f, 0f), t);
                    }
                }
            }
            yield return null;
        }

        if (slashObj != null) Destroy(slashObj);
    }

    private IEnumerator AnimateCircularSlash(GameObject circleObj, SpriteRenderer sr)
    {
        float duration = 0.22f;
        float elapsed = 0f;
        float targetScale = slashRadius * 2.1f;

        while (elapsed < duration)
        {
            if (!GlobalVariables._gameIsPaused)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (circleObj != null)
                {
                    float currentScale = Mathf.Lerp(slashRadius * 0.5f, targetScale, t);
                    circleObj.transform.localScale = new Vector3(currentScale, currentScale, 1f);
                    circleObj.transform.Rotate(0f, 0f, 650f * Time.deltaTime);

                    if (sr != null)
                    {
                        sr.color = Color.Lerp(new Color(0.5f, 0.9f, 1f, 1f), new Color(0.1f, 0.5f, 1f, 0f), t * t);
                    }
                }
            }
            yield return null;
        }

        if (circleObj != null) Destroy(circleObj);
    }

    private Transform GetClosestEnemyInRadius(float searchRadius)
    {
        int count = Physics2D.OverlapCircleNonAlloc(player.transform.position, searchRadius, swordHitBuffer, enemyLayer);
        Transform closest = null;
        float minSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (swordHitBuffer[i] == null) continue;
            Enemy enemy = swordHitBuffer[i].GetComponent<Enemy>();
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

    // --- CRIAÇÃO PROCEDURAL DE TEXTURAS / SPRITES ---

    private static Sprite CreateSlashArcSprite()
    {
        int w = 64;
        int h = 48;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++) tex.SetPixel(x, y, clear);
        }

        // Desenha corte em arco luminoso crescente
        float centerX = 10f;
        float centerY = h / 2f;
        float radius = 34f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

                if (Mathf.Abs(angle) <= 65f)
                {
                    float edgeDist = Mathf.Abs(dist - radius);
                    if (edgeDist <= 3.2f)
                    {
                        float alpha = (1f - edgeDist / 3.2f) * Mathf.Cos((angle / 65f) * (Mathf.PI * 0.45f));
                        Color c = Color.Lerp(new Color(0.4f, 0.8f, 1f, alpha), Color.white, 1f - edgeDist / 2f);
                        tex.SetPixel(x, y, c);
                    }
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.2f, 0.5f), 32f);
    }

    private static Sprite CreateCircleSlashSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++) tex.SetPixel(x, y, clear);
        }

        float center = size / 2f;
        float ringRadius = 24f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float edge = Mathf.Abs(dist - ringRadius);

                if (edge <= 4.0f)
                {
                    float alpha = 1f - (edge / 4.0f);
                    Color c = Color.Lerp(new Color(0.3f, 0.75f, 1f, alpha), Color.white, 1f - edge / 2.5f);
                    tex.SetPixel(x, y, c);
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }

    private static Sprite CreateSwordWaveSprite()
    {
        int w = 48;
        int h = 24;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++) tex.SetPixel(x, y, clear);
        }

        int centerY = h / 2;

        // Desenha lâmina crescente de energia cortante
        for (int x = 0; x < w; x++)
        {
            float normX = (float)x / w;
            float curveX = Mathf.Sin(normX * Mathf.PI) * 10f;

            for (int y = 0; y < h; y++)
            {
                float dy = Mathf.Abs(y - centerY);
                float thickness = (1f - (float)x / w) * 2.8f;
                float targetY = Mathf.Abs(normX * 6f);

                if (dy <= thickness + (1f - normX) * 4f)
                {
                    float alpha = 1f - (dy / 7f);
                    Color c = Color.Lerp(new Color(0.3f, 0.9f, 1f, alpha), Color.white, 1f - normX * 0.8f);
                    tex.SetPixel(x, y, c);
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
    }
}

public class SwordWaveProjectile : MonoBehaviour
{
    private Player player;
    private float damage;
    private float speed;
    private int remainingPierces;
    private float lifeTime = 2.2f;
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
            Vector2 knockback = transform.right * 2.0f;
            enemy.TakeDamage(finalDmg, knockback, isCrit);

            remainingPierces--;
            if (remainingPierces <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
