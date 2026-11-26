using System.Collections.Generic;
using UnityEngine;

public class ExperienceGem : MonoBehaviour
{
    private static List<ExperienceGem> activeGems = new List<ExperienceGem>();
    private static List<ExperienceGem> pool = new List<ExperienceGem>();
    private const int MAX_ACTIVE_GEMS = 120; // Limite máximo de gemas no mapa

    [SerializeField] private int xpValue = 1;
    [SerializeField] private float pullSpeed = 7f;
    [SerializeField] private float acceleration = 12f;

    private Transform playerTransform;
    private Player player;
    private bool isBeingPulled = false;
    private float currentSpeed;
    private SpriteRenderer sr;
    private CircleCollider2D col;

    private static Sprite defaultGemSprite;

    public int XpValue { get => xpValue; set => xpValue = value; }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<CircleCollider2D>();
    }

    void OnEnable()
    {
        isBeingPulled = false;
        currentSpeed = pullSpeed;
        if (!activeGems.Contains(this))
        {
            activeGems.Add(this);
        }
    }

    void OnDisable()
    {
        activeGems.Remove(this);
    }

    void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                player = playerObj.GetComponent<Player>();
            }
        }
    }

    void Update()
    {
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        Vector2 diff = (Vector2)transform.position - (Vector2)playerTransform.position;
        float distanceSqr = diff.sqrMagnitude;
        float magnetRadius = (player != null) ? player.magnetRadius : 3.5f;

        // Ativa atração ao entrar no raio do ímã
        if (!isBeingPulled)
        {
            if (distanceSqr <= magnetRadius * magnetRadius)
            {
                isBeingPulled = true;
            }
        }

        // Voa em direção ao jogador acelerando
        if (isBeingPulled)
        {
            currentSpeed += acceleration * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, currentSpeed * Time.deltaTime);

            if (distanceSqr <= 0.22f) // ~0.47 unidades
            {
                Collect();
            }
        }
    }

    private void Collect()
    {
        if (player != null)
        {
            player.AddExperience(xpValue);
            SoundManager.PlayXpPickup();
        }
        gameObject.SetActive(false); // Retorna ao pool de gemas
    }

    public static ExperienceGem Spawn(Vector3 position, int xp = 1)
    {
        // Se houver gemas demais no mapa, condensa o valor da mais antiga e a recolhe (Gem Compression)
        if (activeGems.Count >= MAX_ACTIVE_GEMS && activeGems.Count > 0)
        {
            ExperienceGem oldest = activeGems[0];
            if (oldest != null)
            {
                xp += oldest.xpValue;
                oldest.gameObject.SetActive(false);
            }
        }

        ExperienceGem gem = GetFromPool();
        gem.transform.position = position;
        gem.xpValue = xp;
        gem.ConfigureVisual(xp);
        gem.gameObject.SetActive(true);

        return gem;
    }

    private void ConfigureVisual(int xp)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (defaultGemSprite == null) defaultGemSprite = CreateDiamondSprite();
        sr.sprite = defaultGemSprite;

        // Cor de acordo com o valor de XP: Azul (1), Verde (5), Vermelho/Dourado (10+)
        if (xp >= 10)
            sr.color = new Color(1f, 0.3f, 0.3f, 1f); // Vermelho
        else if (xp >= 5)
            sr.color = new Color(0.2f, 1f, 0.4f, 1f); // Verde
        else
            sr.color = new Color(0.2f, 0.8f, 1f, 1f); // Azul Ciano
    }

    private static ExperienceGem GetFromPool()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] != null && !pool[i].gameObject.activeInHierarchy)
            {
                return pool[i];
            }
        }

        GameObject gemObj = new GameObject("XP_Gem_Pooled");
        gemObj.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

        SpriteRenderer sr = gemObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;

        CircleCollider2D col = gemObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        ExperienceGem gem = gemObj.AddComponent<ExperienceGem>();
        pool.Add(gem);

        return gem;
    }

    private static Sprite CreateDiamondSprite()
    {
        int size = 16;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color white = Color.white;
        Color edgeColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int distFromCenter = Mathf.Abs(x - size / 2) + Mathf.Abs(y - size / 2);
                if (distFromCenter <= size / 2)
                {
                    if (distFromCenter == size / 2)
                        texture.SetPixel(x, y, edgeColor);
                    else
                        texture.SetPixel(x, y, white);
                }
                else
                {
                    texture.SetPixel(x, y, clear);
                }
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }
}
