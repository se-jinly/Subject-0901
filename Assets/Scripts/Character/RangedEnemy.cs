using UnityEngine;

[RequireComponent(typeof(RangedAttack))]
public class RangedEnemy : Enemy
{
    private RangedAttack _rangedAttack;

    [Header("사거리 유지")]
    [SerializeField] protected float _keepDistance = 5f;
    [SerializeField] private float _tooClose = 3f;
    [Tooltip("회전 속도"), SerializeField] private float _rotateSpeed = 180f;

    protected override void Awake()
    {
        base.Awake();
        _rangedAttack = GetComponent<RangedAttack>();
        _agent.updateRotation = false;
    }
    protected override void TryAttack()
    {
        if (Vector3.Distance(transform.position, _player.position) > _detectRange) return;

        _rangedAttack.Execute();
    }
    protected override void Chase()
    {
        Vector3 dir = _player.position - transform.position;
        dir.y = 0f;
        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _rotateSpeed * Time.deltaTime);
        

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
