using UnityEngine;

public class ExperienceGem : MonoBehaviour
{
    [SerializeField] private int xpValue = 1;
    [SerializeField] private float pullSpeed = 7f;
    [SerializeField] private float acceleration = 12f;

    private Transform playerTransform;
    private Player player;
    private bool isBeingPulled = false;
    private float currentSpeed;

    private static Sprite defaultGemSprite;

    public int XpValue { get => xpValue; set => xpValue = value; }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            player = playerObj.GetComponent<Player>();
        }
        currentSpeed = pullSpeed;
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // Verifica se entrou no raio do Ã­mÃ£ do player
        if (!isBeingPulled && player != null)
        {
            if (distance <= player.magnetRadius)
            {
                isBeingPulled = true;
            }
        }

        // Se estÃ¡ sendo atraÃ­do, voa acelerando atÃ© o player
        if (isBeingPulled)
        {
            currentSpeed += acceleration * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, currentSpeed * Time.deltaTime);

            // Ao encostar no player, coleta o XP
            if (distance <= 0.45f)
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
        Destroy(gameObject);
    }

    public static ExperienceGem Spawn(Vector3 position, int xp = 1)
    {
        GameObject gemObj = new GameObject("XP_Gem");
        gemObj.transform.position = position;
        gemObj.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

        SpriteRenderer sr = gemObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;

        // Se tiver sprite padrÃ£o gerado, usa; senÃ£o gera um losango de cristal
        if (defaultGemSprite == null)
        {
            defaultGemSprite = CreateDiamondSprite();
        }
        sr.sprite = defaultGemSprite;

        // Cor de acordo com o valor de XP: Azul (1), Verde (5), Vermelho/Dourado (10+)
        if (xp >= 10)
            sr.color = new Color(1f, 0.3f, 0.3f, 1f); // Vermelho
        else if (xp >= 5)
            sr.color = new Color(0.2f, 1f, 0.4f, 1f); // Verde
        else
            sr.color = new Color(0.2f, 0.8f, 1f, 1f); // Azul Ciano

        CircleCollider2D col = gemObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        ExperienceGem gem = gemObj.AddComponent<ExperienceGem>();
        gem.xpValue = xp;

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

