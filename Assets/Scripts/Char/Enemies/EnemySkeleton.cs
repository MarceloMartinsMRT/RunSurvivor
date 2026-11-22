using UnityEngine;

public class EnemySkeleton : Enemy
{
    [SerializeField] private Animator animator;
    [SerializeField] private CapsuleCollider2D hitBox;

    protected override void Awake()
    {
        base.Awake();
        _health = 32;
        _speed = 0.8f; // Reduzido de 1.0f para 0.8f
        _damage = 12;
        _attackCooldown = 0.8f;
        _xpReward = 2;
    }
}
