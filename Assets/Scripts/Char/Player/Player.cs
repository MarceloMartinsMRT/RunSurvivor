using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("GameObjects")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Image imageHP;
    [SerializeField] private Image exp;
    [SerializeField] private GameObject aim;

    [Header("Movement Cursor")]
    [SerializeField] private GameObject movementCursor;
    [SerializeField] private float movementCursorDistance = 1.05f;
    [SerializeField] private Color movementCursorColor = new Color(0.25f, 1f, 0.45f, 0.95f);

    [Header("PlayerStats")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float radius = 7f;

    [Header("Powers")]
    [SerializeField] private GameObject power1;
    [SerializeField] private float power1FireRate = 1.4f;

    [Header("Survivor RPG Stats")]
    public float maxHealth = 100f;
    public float currentHealthPoints = 100f;
    public float currentPower = 1f;
    public float damageMultiplier = 1f;
    public float cooldownMultiplier = 1f;
    public int extraProjectiles = 0;
    public float magnetRadius = 3.5f;
    public float critChance = 0.10f; // 10% de chance base
    public float critMultiplier = 2.0f; // 2.0x dano crítico

    [Header("Progression")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 10;

    private Rigidbody2D rig;
    private Vector2 _direction;
    private float power1TimeCount;
    private float angle;
    private bool isInvulnerable = false;
    private bool isDead = false;
    private Vector2 lastFacingDirection = Vector2.right;
    private SpriteRenderer movementCursorRenderer;
    private static Sprite cachedArrowSprite;

    public Vector2 Direction { get => _direction; set => _direction = value; }
    public Vector2 LastFacingDirection => lastFacingDirection;
    public LayerMask EnemyLayer => enemyLayer;

    void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        currentHealthPoints = maxHealth;
        UpdateHealthUI();
        UpdateExpUI();

        // Se exp image nao estiver atribuida no Player, procura na cena
        if (exp == null)
        {
            GameObject expObj = GameObject.Find("EXP");
            if (expObj != null) exp = expObj.GetComponent<Image>();
        }

        // Inicializa o cursor de movimento se não foi configurado manualmente
        if (movementCursor == null)
        {
            CreateMovementCursor();
        }
        else
        {
            movementCursorRenderer = movementCursor.GetComponentInChildren<SpriteRenderer>();
        }
    }

    void Update()
    {
        if (GlobalVariables._gameIsPaused || isDead) return;

        OnInput();
        UpdateAimAndClosestEnemy();
        UpdateMovementCursor();
        PowerOneShoot();
    }

    private void FixedUpdate()
    {
        if (GlobalVariables._gameIsPaused || isDead) return;
        OnMove();
    }

    void OnInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _direction = new Vector2(h, v).normalized;

        if (_direction.sqrMagnitude > 0.01f)
        {
            lastFacingDirection = _direction;
        }
    }

    void OnMove()
    {
        rig.MovePosition(rig.position + speed * Time.fixedDeltaTime * _direction);

        if (_direction.x < 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = true;
        }
        else if (_direction.x > 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
        }

        if (animator != null)
        {
            animator.SetInteger("animHero", _direction.sqrMagnitude > 0 ? 1 : 0);
        }
    }

    void UpdateAimAndClosestEnemy()
    {
        Transform closestEnemy = GetClosestEnemy();

        if (closestEnemy != null)
        {
            Vector3 diff = closestEnemy.position - transform.position;
            angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        }
        else
        {
            // Se nenhum inimigo estiver no raio, mira na direção de movimento
            angle = Mathf.Atan2(lastFacingDirection.y, lastFacingDirection.x) * Mathf.Rad2Deg;
        }

        if (aim != null)
        {
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            aim.transform.rotation = Quaternion.RotateTowards(aim.transform.rotation, targetRotation, Time.deltaTime * 720f);
        }
    }

    void UpdateMovementCursor()
    {
        if (movementCursor == null) return;

        bool isMoving = _direction.sqrMagnitude > 0.01f;
        Vector2 moveDir = isMoving ? _direction : lastFacingDirection;

        float moveAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, moveAngle);

        // Rotação rápida e responsiva acompanhando as teclas de movimento
        movementCursor.transform.rotation = Quaternion.RotateTowards(
            movementCursor.transform.rotation,
            targetRotation,
            Time.deltaTime * 1440f
        );

        // Feedback de transparência: 100% visível em movimento, sutil quando parado
        if (movementCursorRenderer != null)
        {
            Color c = movementCursorColor;
            c.a = isMoving ? movementCursorColor.a : (movementCursorColor.a * 0.45f);
            movementCursorRenderer.color = c;
        }
    }

    void CreateMovementCursor()
    {
        GameObject pivot = new GameObject("MovementCursorPivot");
        pivot.transform.SetParent(transform);
        pivot.transform.localPosition = Vector3.zero;

        GameObject arrow = new GameObject("MovementArrow");
        arrow.transform.SetParent(pivot.transform);
        arrow.transform.localPosition = new Vector3(movementCursorDistance, 0, 0);
        arrow.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

        movementCursorRenderer = arrow.AddComponent<SpriteRenderer>();
        if (cachedArrowSprite == null)
        {
            cachedArrowSprite = CreateChevronArrowSprite();
        }
        movementCursorRenderer.sprite = cachedArrowSprite;
        movementCursorRenderer.sortingOrder = 6;
        movementCursorRenderer.color = movementCursorColor;

        movementCursor = pivot;
    }

    private static Sprite CreateChevronArrowSprite()
    {
        int w = 20;
        int h = 20;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color fill = Color.white;
        Color outline = new Color(0.1f, 0.15f, 0.1f, 0.9f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                tex.SetPixel(x, y, clear);
            }
        }

        int centerY = h / 2;

        // Desenha um chevron apontando para a direita (+X)
        for (int x = 3; x <= 16; x++)
        {
            float maxDy = (16 - x) * 0.65f;
            float innerCut = (x < 10) ? (10 - x) * 1.0f : 0f;

            for (int y = 0; y < h; y++)
            {
                float dy = Mathf.Abs(y - centerY);

                if (dy <= maxDy && dy >= innerCut)
                {
                    // Preenchimento interno
                    tex.SetPixel(x, y, fill);
                }
                else if (dy <= maxDy + 1 && dy >= innerCut - 1 && dy > 0)
                {
                    // Borda externa para contraste
                    if (tex.GetPixel(x, y).a == 0)
                    {
                        tex.SetPixel(x, y, outline);
                    }
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }

    Transform GetClosestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        Transform closest = null;
        float minDistanceSqr = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;
            Enemy enemy = hits[i].GetComponent<Enemy>();
            if (enemy != null && enemy.IsDead) continue;

            float distSqr = (hits[i].transform.position - transform.position).sqrMagnitude;
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                closest = hits[i].transform;
            }
        }

        return closest;
    }

    void PowerOneShoot()
    {
        if (power1 == null) return;

        power1TimeCount += Time.deltaTime;
        float actualFireRate = Mathf.Max(0.2f, power1FireRate * cooldownMultiplier);

        if (power1TimeCount >= actualFireRate)
        {
            power1TimeCount = 0f;
            int totalShots = 1 + extraProjectiles;

            if (totalShots == 1)
            {
                Instantiate(power1, transform.position, Quaternion.Euler(0, 0, angle));
            }
            else
            {
                // Disparo em leque
                float spreadAngle = 15f;
                float startAngle = angle - (spreadAngle * (totalShots - 1) / 2f);

                for (int i = 0; i < totalShots; i++)
                {
                    float currentAngle = startAngle + (i * spreadAngle);
                    Instantiate(power1, transform.position, Quaternion.Euler(0, 0, currentAngle));
                }
            }
        }
    }

    public int CalculateDamage(float baseDamage, out bool isCrit)
    {
        float totalDamage = (baseDamage * damageMultiplier) + (currentPower - 1f) * 4f;
        isCrit = Random.value < critChance;
        if (isCrit)
        {
            totalDamage *= critMultiplier;
        }
        return Mathf.Max(1, Mathf.RoundToInt(totalDamage));
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable || isDead) return;

        currentHealthPoints -= damage;
        DamagePopup.CreatePlayerDamage(transform.position, damage);
        if (currentHealthPoints <= 0)
        {
            currentHealthPoints = 0;
            UpdateHealthUI();
            Die();
            return;
        }

        UpdateHealthUI();
        StartCoroutine(InvulnerabilityRoutine());
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        if (spriteRenderer != null)
        {
            for (int i = 0; i < 4; i++)
            {
                spriteRenderer.color = new Color(1f, 0.4f, 0.4f, 0.5f);
                yield return new WaitForSeconds(0.08f);
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.08f);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.4f);
        }
        isInvulnerable = false;
    }

    public void AddExperience(int amount)
    {
        if (isDead) return;

        currentXP += amount;
        if (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            currentLevel++;
            xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.35f + 5);

            UpdateExpUI();

            if (LevelUpManager.Instance != null)
            {
                LevelUpManager.Instance.ShowLevelUp();
            }
        }
        else
        {
            UpdateExpUI();
        }
    }

    public void Heal(float amount)
    {
        currentHealthPoints = Mathf.Min(maxHealth, currentHealthPoints + amount);
        UpdateHealthUI();
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        currentHealthPoints += amount;
        UpdateHealthUI();
    }

    public void IncreaseSpeed(float percentage)
    {
        speed += speed * (percentage / 100f);
    }

    public void IncreaseDamage(float percentage)
    {
        damageMultiplier += percentage / 100f;
    }

    public void ReduceCooldown(float percentage)
    {
        cooldownMultiplier = Mathf.Max(0.2f, cooldownMultiplier - (percentage / 100f));
    }

    public void IncreaseMagnetRadius(float percentage)
    {
        magnetRadius += magnetRadius * (percentage / 100f);
    }

    public void AddExtraProjectile(int count = 1)
    {
        extraProjectiles += count;
    }

    public void IncreaseCritChance(float percentage)
    {
        critChance += percentage / 100f;
    }

    public void IncreaseCritMultiplier(float amount)
    {
        critMultiplier += amount;
    }

    private void UpdateHealthUI()
    {
        if (imageHP != null)
        {
            imageHP.fillAmount = currentHealthPoints / maxHealth;
        }
    }

    private void UpdateExpUI()
    {
        if (exp != null)
        {
            exp.fillAmount = (float)currentXP / xpToNextLevel;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("heroHurt");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    public void setFireRate(float fireRate)
    {
        power1FireRate = power1FireRate - (power1FireRate * (fireRate / 100f));
    }

    public void increasePower(float power)
    {
        currentPower += power;
    }

    public void decreasePower(float power)
    {
        currentPower = Mathf.Max(1f, currentPower - power);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }
}
