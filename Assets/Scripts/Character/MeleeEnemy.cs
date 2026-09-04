using UnityEngine;
[RequireComponent(typeof(MeleeAttack))]
public class MeleeEnemy : Enemy
{
    private MeleeAttack _meleeAttack;
    protected override void Awake()
    {
        base.Awake();
        _meleeAttack = GetComponent<MeleeAttack>();
    }

    protected override void TryAttack()
    {
        if (Vector3.Distance(transform.position, _player.position) > _detectRange) return;
        _animator.SetTrigger("Attack");
        _meleeAttack.Execute();
    }

    protected override void Chase()
    {
        _agent.SetDestination(_player.position);

    }
}
