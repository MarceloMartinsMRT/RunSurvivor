using UnityEngine;

public class EnemyFlyingEye : Enemy
{
    [SerializeField] private Animator animator;
    [SerializeField] private CapsuleCollider2D hitBox;

    protected override void Awake()
    {
        base.Awake();
        _health = 16;
        _speed = 2.2f; // Reduzido de 3.0f para 2.2f
        _damage = 8;
        _attackCooldown = 0.5f;
        _xpReward = 1;
    }
}
