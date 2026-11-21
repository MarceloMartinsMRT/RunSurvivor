using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected int _health = 25;
    [SerializeField] protected int _originalHealth;
    [SerializeField] protected float _speed = 2.5f;
    [SerializeField] protected int _damage = 10;
    [SerializeField] protected float _attackCooldown = 0.8f;
    [SerializeField] protected int _xpReward = 1;

    [Header("Components")]
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Collider2D col;

    protected bool _isDead = false;
    protected float _originalSpeed;
    protected float _lastAttackTime = -999f;
    protected Transform playerTransform;
    protected Player player;

    public Waves waves;

    public int Health { get => _health; set => _health = value; }
    public float Speed { get => _speed; set => _speed = value; }
    public bool IsDead { get => _isDead; set => _isDead = value; }
    public int OriginalHealth { get => _originalHealth; private set => _originalHealth = value; }
    public int Damage { get => _damage; set => _damage = value; }
    public int XpReward { get => _xpReward; set => _xpReward = value; }

    protected virtual void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (col == null) col = GetComponent<Collider2D>();
    }

    protected virtual void Start()
    {
        _originalSpeed = _speed;
        _originalHealth = _health;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            player = playerObj.GetComponent<Player>();
        }

        if (waves == null)
        {
            waves = FindObjectOfType<Waves>();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (GlobalVariables._gameIsPaused || _isDead || playerTransform == null) return;

        Vector2 toPlayer = (Vector2)playerTransform.position - rb.position;
        Vector2 direction = toPlayer.normalized;

        // Movimentação contínua em direção ao jogador
        rb.MovePosition(rb.position + direction * _speed * Time.fixedDeltaTime);

        // Orientação do sprite
        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }

        // Checagem de dano de contato contínuo
        if (toPlayer.sqrMagnitude <= 1.0f)
        {
            TryDealContactDamage();
        }
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TryDealContactDamage();
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            TryDealContactDamage();
        }
    }

    protected void TryDealContactDamage()
    {
        if (_isDead || player == null) return;

        if (Time.time >= _lastAttackTime + _attackCooldown)
        {
            _lastAttackTime = Time.time;
            player.TakeDamage(_damage);
        }
    }

    public virtual void TakeDamage(int damage, Vector2 knockbackDir = default)
    {
        if (_isDead) return;

        _health -= damage;

        // Feedback visual rápido de dano (flash vermelho)
        if (gameObject.activeInHierarchy && spriteRenderer != null)
        {
            StartCoroutine(FlashHurt());
        }

        // Leve knockback para afastar o inimigo
        if (knockbackDir != default && rb != null)
        {
            rb.position += knockbackDir.normalized * 0.25f;
        }

        if (_health <= 0)
        {
            Die();
        }
    }

    public void TakeHit(int damage)
    {
        TakeDamage(damage);
    }

    protected IEnumerator FlashHurt()
    {
        if (spriteRenderer == null) yield break;
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.08f);
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    public virtual void Die()
    {
        if (_isDead) return;
        _isDead = true;

        if (col != null) col.enabled = false;

        // Dropar gema de XP
        ExperienceGem.Spawn(transform.position, _xpReward);

        // Notificar contadores de onda e kills
        EnemyKilled();

        // Destruir imediatamente
        Destroy(gameObject);
    }

    public void EnemyKilled()
    {
        if (waves != null)
        {
            waves.EnemyKilled();
        }
    }

    public void ChangeSpeed(float percentage)
    {
        if (_speed == _originalSpeed)
            _speed = _speed - (_speed * percentage);
    }

    // Mantido por compatibilidade
    public void SpriteDirection(Vector2 enemyDirection, SpriteRenderer sr)
    {
        if (sr == null) return;
        if (enemyDirection.x > 0) sr.flipX = false;
        else if (enemyDirection.x < 0) sr.flipX = true;
    }

    public Vector2 CalculateMovement(Vector2 enemyDirection, float speed)
    {
        return speed * Time.deltaTime * enemyDirection.normalized;
    }
}
