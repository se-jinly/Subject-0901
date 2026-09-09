using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeleeAttack))]
public class BossEnemy : Enemy
{
    [Header("패턴")]
    [SerializeField] private float _recoveryTime = 0.8f;
    [Tooltip("공격 모션 시간"), SerializeField] private float _windupTime = 0.5f;
    [SerializeField] private float _patternInterval = 1.5f;
    private float _patternTimer;
    private bool IsMeleeAttack => _patternTimer > 0f;
    private Coroutine _patternRoutine;
    private bool IsPatternRunning => _patternRoutine != null;

    [Header("돌진")]
    [SerializeField] private float _dashSpeed = 12f;
    [SerializeField] private float _dashDuration = 0.7f;
    [SerializeField] private int _dashDamage = 4;
    [SerializeField] private float _dashRange = 2f;

    [Header("범위공격")]
    [SerializeField] private float _slamRange = 6f;
    [SerializeField] private int _slamDamage = 6;



    private MeleeAttack _meleeAttack;

    protected override void Awake()
    {
        base.Awake();
        _patternTimer = _patternInterval;
        _meleeAttack = GetComponent<MeleeAttack>();
    }
    protected override void TryAttack()
    {
        if (IsPatternRunning) return;
        _patternTimer -= Time.deltaTime;
        if (IsMeleeAttack)
        {
            if (Vector3.Distance(_player.position, transform.position) >= _detectRange) return;
            _animator.SetTrigger("Attack");
            _meleeAttack.Execute();
            return;
        }
        _patternRoutine = StartCoroutine(PatternRoutine());
    }

    private IEnumerator PatternRoutine()
    {
        //_animator.SetTrigger(""); 아직 없음.
        yield return new WaitForSeconds(_windupTime);

        int randomPattern = Random.Range(0, 2);
        if(randomPattern == 0)
        {
            yield return StartCoroutine(DashRoutine());
        } else
        {
            yield return StartCoroutine(SlamRoutine());
        }
        yield return new WaitForSeconds(_recoveryTime);

        _patternTimer = _patternInterval;
        _patternRoutine = null;
    }

    private IEnumerator SlamRoutine()
    {
        if(Vector3.Distance(_player.position, transform.position) < _slamRange)
        {
            Vector3 dir = _player.position - transform.position;
            dir.y = 0f;
            dir.Normalize();
            _player.TryGetComponent<IDamageable>(out IDamageable target);
            target.TakeDamage(_slamDamage, dir);
        }
        yield return null;
    }

    private IEnumerator DashRoutine()
    {
        Vector3 dir = _player.position - transform.position;
        dir.y = 0;
        dir.Normalize();

        bool canHit = true;
        float t = 0f;

        while (t < _dashDuration)
        {
            t += Time.deltaTime;
            _agent.Move(dir * _dashSpeed * Time.deltaTime);

            if (canHit && Vector3.Distance(_player.position, transform.position) <= _dashRange)
            {
                _player.TryGetComponent<IDamageable>(out IDamageable target);
                target.TakeDamage(_dashDamage, dir);
                canHit = false;
            }

            yield return null;
        }
    }

    protected override void Chase()
    {
        if(IsPatternRunning)
        {
            _agent.SetDestination(transform.position);
            return;
        }
        _agent.SetDestination(_player.position);
    }
}
