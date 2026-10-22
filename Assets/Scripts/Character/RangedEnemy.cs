using UnityEngine;

[RequireComponent(typeof(RangedAttack))]
public class RangedEnemy : Enemy
{
    private RangedAttack _rangedAttack;

    [Header("사거리 유지")]
    [SerializeField] protected float _keepDistance = 5f;
    [SerializeField] private float _tooClose = 3f;
    [Tooltip("회전 속도"), SerializeField] private float _rotateSpeed = 180f;

    [Header("부대")]
    [Tooltip("부대 일제 사격과 별개로 스스로도 계속 쏨 (부대장용)"), SerializeField] private bool _fireIndependently;

    protected override void Awake()
    {
        base.Awake();
        _rangedAttack = GetComponent<RangedAttack>();
        _agent.updateRotation = false;
    }
    protected override void TryAttack()
    {
        if (_squad != null && !_fireIndependently) return;
        if (Vector3.Distance(transform.position, _player.position) > _detectRange) return;

        _rangedAttack.Execute();
    }
    // 부대 신호에 맞춰 발사
    public void FireVolley() => _rangedAttack.Execute();

    protected override void FollowSlot()
    {
        base.FollowSlot();
        RotateToPlayer();
    }

    private void RotateToPlayer()
    {
        Vector3 dir = _player.position - transform.position;
        dir.y = 0f;
        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _rotateSpeed * Time.deltaTime);
    }

    protected override void Chase()
    {
        RotateToPlayer();

        float distance = Vector3.Distance(transform.position, _player.position);
        if (distance <= _tooClose)
        {
            _agent.SetDestination(GetKeepDistancePoint(_keepDistance));
        }
        else if (distance > _keepDistance)
        {
            _agent.SetDestination(_player.position);
        }
        else
        {
            _agent.SetDestination(transform.position);
        }
    }
    private Vector3 GetKeepDistancePoint(float target)
    {
        Vector3 dir = transform.position - _player.position;
        dir.y = 0f;
        dir.Normalize();
        Vector3 movePoint = _player.position + dir * target;
        return movePoint;
    }
}
