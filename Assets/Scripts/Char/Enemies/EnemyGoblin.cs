using UnityEngine;

public class EnemyGoblin : Enemy
{
    [SerializeField] private Animator animator;
    [SerializeField] private CapsuleCollider2D hitBox;

    protected override void Awake()
    {
        base.Awake();
        _health = 25;
        _speed = 1.5f; // Reduzido de 2.0f para 1.5f
        _damage = 10;
        _attackCooldown = 0.7f;
        _xpReward = 1;
    }
}
