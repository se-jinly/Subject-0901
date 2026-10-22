using UnityEngine;
[RequireComponent(typeof(MeleeAttack))]
public class MeleeEnemy : Enemy
{
    private MeleeAttack _meleeAttack;

    [Header("부대")]
    [Tooltip("슬롯에서 이 거리보다 멀어지면 대형으로 복귀 (원거리를 지키는 범위)"), SerializeField] private float _leashDistance = 2f;
    [Tooltip("플레이어가 슬롯에서 이 거리 안일 때만 대형을 벗어나 달려듦"), SerializeField] private float _engageRange = 5f;
    [Tooltip("대형 정렬 시 플레이어를 바라보는 회전 속도"), SerializeField] private float _rotateSpeed = 360f;
    private bool _isReturning;

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

    // 부대 기준 거리로 제한: 슬롯에서 너무 멀어지면 돌아가고, 슬롯 근처에 오면 다시 달려들 수 있음
    protected override bool ShouldLeaveSlot()
    {
        if (_squad.IsCharging) return true;
        if (_squad.IsStandoff) return false;
        Vector3 slot = _squad.GetSlotPosition(this);
        float fromSlot = Vector3.Distance(transform.position, slot);
        if (fromSlot > _leashDistance) _isReturning = true;
        else if (fromSlot <= _leashDistance * 0.3f) _isReturning = false;
        if (_isReturning) return false;
        return Vector3.Distance(slot, _player.position) <= _engageRange;
    }

    // 대형 안에서는 공격 방향이 맞도록 플레이어 쪽을 바라봄
    protected override void FollowSlot()
    {
        base.FollowSlot();
        Vector3 dir = _player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), _rotateSpeed * Time.deltaTime);
    }
}
