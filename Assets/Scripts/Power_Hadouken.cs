using UnityEngine;

public class Power_Hadouken : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed = 12f;
    [SerializeField] private float power = 15f;
    [SerializeField] private int totalHits = 3;
    [SerializeField] private float lifeTime = 3.5f;

    private int hits = 0;
    private Player player;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<Player>();
            if (player != null)
            {
                power = power * player.damageMultiplier + (player.currentPower - 1f) * 5f;
            }
        }
    }

    void Update()
    {
        if (GlobalVariables._gameIsPaused) return;

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        if (GlobalVariables._gameIsPaused) return;

        Vector2 movement = (Vector2)transform.right * speed * Time.fixedDeltaTime;
        if (rb != null)
        {
            rb.MovePosition(rb.position + movement);
        }
        else
        {
            transform.position += (Vector3)movement;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Enemy>(out Enemy enemy))
        {
            hits++;
            enemy.TakeDamage((int)power, transform.right);

            if (hits >= totalHits)
            {
                Destroy(gameObject);
            }
        }
    }
}
