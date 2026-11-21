using UnityEngine;

public class EnemySkeleton : Enemy
{
    [SerializeField] private Animator animator;
    [SerializeField] private CapsuleCollider2D hitBox;

    protected override void Awake()
    {
        base.Awake();
        _health = 32;
        _speed = 1.0f; // Médio-lento (2x Mushroom), 20% da velocidade do Player (5.0)
        _damage = 12;
        _attackCooldown = 0.8f;
        _xpReward = 2;
    }
}
