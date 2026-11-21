using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Whip : MonoBehaviour
{
    [Header("Stats")]
    public int level = 0; // 0 = desativado
    public float baseInterval = 1.35f;
    public float whipDamage = 26f;
    public float whipRange = 3.2f;
    public float whipHeight = 1.6f;
    public bool strikesBackwards = false;
    public int frontalStrikes = 1;

    private float timer = 0f;
    private Player player;
    private LayerMask enemyLayer;
    private static Sprite slashSprite;

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
            StartCoroutine(ExecuteWhipAttack());
        }
    }

    public void UnlockOrUpgrade()
    {
        level++;
        if (level == 1)
        {
            whipDamage = 26f;
            whipRange = 3.2f;
            baseInterval = 1.35f;
            strikesBackwards = false;
            frontalStrikes = 1;
        }
        else if (level == 2)
        {
            whipDamage = 34f;
            whipRange = 4.0f;
            whipHeight = 1.8f;
        }
        else if (level == 3)
        {
            whipDamage = 42f;
            strikesBackwards = true; // Chicote duplo: frente e trás!
        }
        else if (level == 4)
        {
            whipDamage = 54f;
            baseInterval = 1.1f;
            whipRange = 4.6f;
        }
        else
        {
            whipDamage += 15f;
            frontalStrikes = 2; // Chicote triplo devastador!
            whipRange += 0.5f;
            baseInterval = Mathf.Max(0.5f, baseInterval - 0.08f);
        }
    }

    private IEnumerator ExecuteWhipAttack()
    {
        Vector2 facing = player.LastFacingDirection;
        float dirX = (facing.x < 0) ? -1f : 1f;

        // Golpe(s) frontal(is)
        for (int i = 0; i < frontalStrikes; i++)
        {
            PerformSlash(dirX);
            if (i < frontalStrikes - 1)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        // Golpe traseiro se desbloqueado
        if (strikesBackwards)
        {
            yield return new WaitForSeconds(0.12f);
            PerformSlash(-dirX);
        }
    }

    private void PerformSlash(float dirX)
    {
        if (slashSprite == null)
        {
            slashSprite = CreateSlashSprite();
        }

        Vector3 slashCenter = player.transform.position + new Vector3(dirX * (whipRange * 0.5f), 0.1f, 0f);
        Vector2 boxSize = new Vector2(whipRange, whipHeight);

        // Visual do golpe de chicote
        GameObject slashObj = new GameObject("WhipSlash");
        slashObj.transform.position = slashCenter;
        slashObj.transform.localScale = new Vector3(dirX * whipRange * 0.7f, whipHeight * 1.1f, 1f);

        SpriteRenderer sr = slashObj.AddComponent<SpriteRenderer>();
        sr.sprite = slashSprite;
        sr.sortingOrder = 8;
        sr.color = new Color(0.95f, 0.95f, 1f, 0.95f);

        // Detecção de dano em área com knockback
        Collider2D[] hits = Physics2D.OverlapBoxAll(slashCenter, boxSize, 0f, enemyLayer);
        HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;
            Enemy enemy = hits[i].GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead && !hitEnemies.Contains(enemy))
            {
                hitEnemies.Add(enemy);
                bool isCrit = false;
                int dmg = player != null ? player.CalculateDamage(whipDamage, out isCrit) : (int)whipDamage;
                Vector2 knockback = new Vector2(dirX * 1.8f, 0.2f);
                enemy.TakeDamage(dmg, knockback, isCrit);
            }
        }

        StartCoroutine(AnimateSlash(slashObj, sr, dirX));
    }

    private IEnumerator AnimateSlash(GameObject slashObj, SpriteRenderer sr, float dirX)
    {
        float duration = 0.15f;
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
                    slashObj.transform.localScale = new Vector3(initialScale.x * (1f + t * 0.3f), initialScale.y * (1f - t * 0.2f), 1f);
                    if (sr != null)
                    {
                        sr.color = new Color(1f, 0.9f, 0.5f, 1f - t);
                    }
                }
            }
            yield return null;
        }

        if (slashObj != null) Destroy(slashObj);
    }

    private static Sprite CreateSlashSprite()
    {
        int w = 48;
        int h = 24;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                tex.SetPixel(x, y, clear);
            }
        }

        int centerY = h / 2;

        // Desenhar arco de chicotada estilizado com brilho
        for (int x = 4; x < w - 4; x++)
        {
            float normX = (float)x / w;
            // Curvatura parabólica
            float curveY = centerY + Mathf.Sin(normX * Mathf.PI) * 4.5f;

            for (int y = 0; y < h; y++)
            {
                float distY = Mathf.Abs(y - curveY);
                if (distY <= 2.2f)
                {
                    float alpha = 1f - (distY / 2.2f);
                    Color c = Color.Lerp(new Color(1f, 0.95f, 0.6f, alpha), Color.white, 1f - distY / 1.5f);
                    tex.SetPixel(x, y, c);
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
    }
}
