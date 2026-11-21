using UnityEngine;

public class EnemyFlyingEye : Enemy
{
    [SerializeField] private Animator animator;
    [SerializeField] private CapsuleCollider2D hitBox;

    protected override void Awake()
    {
        base.Awake();
        _health = 16;
        _speed = 3.0f; // Rápido (6x Mushroom), 60% da velocidade do Player (5.0)
        _damage = 8;
        _attackCooldown = 0.5f;
        _xpReward = 1;
    }
}
