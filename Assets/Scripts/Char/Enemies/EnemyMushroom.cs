using UnityEngine;

public class EnemyMushroom : Enemy
{
    [SerializeField] private Animator animator;
    [SerializeField] private CapsuleCollider2D hitBox;

    protected override void Awake()
    {
        base.Awake();
        _health = 45;
        _speed = 0.4f; // Reduzido de 0.5f para 0.4f
        _damage = 15;
        _attackCooldown = 0.9f;
        _xpReward = 2;
    }
}
