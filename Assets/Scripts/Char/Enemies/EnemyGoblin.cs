using UnityEngine;

public class EnemyGoblin : Enemy
{
    [SerializeField] private Animator animator;
    [SerializeField] private CapsuleCollider2D hitBox;

    protected override void Awake()
    {
        base.Awake();
        _health = 25;
        _speed = 2.0f; // Médio-rápido (4x Mushroom), 40% da velocidade do Player (5.0)
        _damage = 10;
        _attackCooldown = 0.7f;
        _xpReward = 1;
    }
}
