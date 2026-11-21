using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    public static List<TreasureChest> activeChests = new List<TreasureChest>();

    [Header("Chest Settings")]
    public int upgradeCount = 3; // Quantidade de melhorias que o baú concede (1 a 3)

    private Transform playerTransform;
    private Player player;
    private bool isCollected = false;
    private Vector3 basePosition;
    private float floatTimer = 0f;
    private float currentSpeed = 6f;

    private static Sprite cachedChestSprite;
    private static Sprite cachedAuraSprite;

    void OnEnable()
    {
        if (!activeChests.Contains(this)) activeChests.Add(this);
    }

    void OnDisable()
    {
        activeChests.Remove(this);
    }

    void OnDestroy()
    {
        activeChests.Remove(this);
    }

    void Start()
    {
        basePosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            player = playerObj.GetComponent<Player>();
        }
    }

    void Update()
    {
        if (GlobalVariables._gameIsPaused || isCollected || playerTransform == null) return;

        floatTimer += Time.deltaTime;
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // Se estiver dentro do raio de atração do jogador, voa até ele
        float attractRadius = (player != null) ? Mathf.Max(2.5f, player.magnetRadius) : 2.5f;
        if (distance <= attractRadius)
        {
            currentSpeed += 14f * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, currentSpeed * Time.deltaTime);
        }
        else
        {
            // Flutua suavemente para cima e para baixo
            transform.position = basePosition + new Vector3(0, Mathf.Sin(floatTimer * 3.5f) * 0.14f, 0);
        }

        // Ao encostar no jogador, abre o baú
        if (distance <= 0.65f)
        {
            Collect();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isCollected)
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (isCollected) return;
        isCollected = true;

        if (LevelUpManager.Instance != null)
        {
            LevelUpManager.Instance.OpenTreasureChest(upgradeCount);
        }

        Destroy(gameObject);
    }

    public static TreasureChest Spawn(Vector3 position, int upgrades = 3)
    {
        GameObject chestObj = new GameObject("TreasureChest");
        chestObj.transform.position = position;
        chestObj.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

        // 1. Sprite do Baú
        SpriteRenderer sr = chestObj.AddComponent<SpriteRenderer>();
        if (cachedChestSprite == null)
        {
            cachedChestSprite = CreateChestSprite();
        }
        sr.sprite = cachedChestSprite;
        sr.sortingOrder = 9;

        // 2. Halo / Brilho dourado ao fundo
        GameObject haloObj = new GameObject("ChestAura");
        haloObj.transform.SetParent(chestObj.transform, false);
        haloObj.transform.localPosition = Vector3.zero;
        haloObj.transform.localScale = new Vector3(1.7f, 1.7f, 1f);

        SpriteRenderer auraSr = haloObj.AddComponent<SpriteRenderer>();
        if (cachedAuraSprite == null)
        {
            cachedAuraSprite = CreateAuraSprite();
        }
        auraSr.sprite = cachedAuraSprite;
        auraSr.color = new Color(1f, 0.85f, 0.2f, 0.45f);
        auraSr.sortingOrder = 8;

        // 3. Colisor de gatilho
        CircleCollider2D col = chestObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        TreasureChest chest = chestObj.AddComponent<TreasureChest>();
        chest.upgradeCount = Mathf.Clamp(upgrades, 1, 3);

        return chest;
    }

    private static Sprite CreateChestSprite()
    {
        int w = 24;
        int h = 20;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color wood = new Color(0.6f, 0.35f, 0.12f, 1f);
        Color woodDark = new Color(0.42f, 0.22f, 0.08f, 1f);
        Color gold = new Color(1f, 0.85f, 0.15f, 1f);
        Color goldLight = new Color(1f, 0.98f, 0.5f, 1f);
        Color iron = new Color(0.2f, 0.2f, 0.25f, 1f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                tex.SetPixel(x, y, clear);
            }
        }

        // Desenhar baú de tesouro dourado
        for (int x = 2; x <= 21; x++)
        {
            for (int y = 2; y <= 17; y++)
            {
                // Borda externa
                if (x == 2 || x == 21 || y == 2 || y == 17)
                {
                    tex.SetPixel(x, y, iron);
                }
                // Tiras metálicas douradas
                else if (x == 4 || x == 19 || x == 11 || x == 12 || y == 10)
                {
                    tex.SetPixel(x, y, (x == 11 || y == 10) ? goldLight : gold);
                }
                // Madeira interna
                else
                {
                    tex.SetPixel(x, y, (y > 10) ? wood : woodDark);
                }
            }
        }

        // Fechadura central iluminada
        for (int x = 10; x <= 13; x++)
        {
            for (int y = 8; y <= 11; y++)
            {
                tex.SetPixel(x, y, goldLight);
            }
        }
        tex.SetPixel(11, 9, iron);
        tex.SetPixel(12, 9, iron);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }

    private static Sprite CreateAuraSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = new Color(0, 0, 0, 0);
        Color goldAura = new Color(1f, 0.85f, 0.2f, 0.8f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                if (dist <= radius)
                {
                    float factor = 1f - (dist / radius);
                    Color c = goldAura;
                    c.a *= factor * factor;
                    tex.SetPixel(x, y, c);
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
