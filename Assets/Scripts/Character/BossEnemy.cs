using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MeleeAttack))]
public class BossEnemy : Enemy
{
    [Header("패턴")]
    [SerializeField] private float _recoveryTime = 0.8f;
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
    [Tooltip("공격 모션 시간"), SerializeField] private float _dashWindupTime = 0.5f;

    [Header("소환")]
    [SerializeField] private MeleeEnemy _summonPrefab;
    [Tooltip("한 번에 소환하는 수"), SerializeField] private int _summonCount = 4;
    [Tooltip("보스 주변 소환 반경"), SerializeField] private float _summonRadius = 2.5f;
    [Tooltip("동시에 살아있을 수 있는 소환 졸개 최대 수"), SerializeField] private int _maxSummons = 8;
    [Tooltip("공격 모션 시간"), SerializeField] private float _summonWindupTime = 0.5f;
    private readonly List<Enemy> _minions = new();
    // 소환된 졸개를 웨이브가 알 수 있도록 알림
    public event System.Action<Enemy> Summoned;


    private MeleeAttack _meleeAttack;

    protected override void Awake()
    {
        base.Awake();
        _patternTimer = _patternInterval;
        _meleeAttack = GetComponent<MeleeAttack>();
        BossHealthBar.Instance.SetHealth(_currentHp, _maxHp);
        BossHealthBar.Instance.Show();
    }

    public override void TakeDamage(int amount, Vector3 dir)
    {
        base.TakeDamage(amount, dir);
        BossHealthBar.Instance.SetHealth(_currentHp, _maxHp);
        if (_currentHp <= 0)
        {
            BossHealthBar.Instance.Hide();
            // 보스가 죽으면 남은 졸개도 함께 사라짐
            foreach (Enemy minion in _minions)
            {
                if (minion != null) Destroy(minion.gameObject);
            }
            _minions.Clear();
        }
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

        int randomPattern = Random.Range(0, 2);
        if(randomPattern == 0)
        {
            yield return new WaitForSeconds(_dashWindupTime);
            yield return StartCoroutine(DashRoutine());
        } else
        {
            yield return new WaitForSeconds(_summonWindupTime);
            yield return StartCoroutine(SummonRoutine());
        }
        yield return new WaitForSeconds(_recoveryTime);

        _patternTimer = _patternInterval;
        _patternRoutine = null;
    }

    private IEnumerator SummonRoutine()
    {
        _minions.RemoveAll(m => m == null);
        int count = Mathf.Min(_summonCount, _maxSummons - _minions.Count);
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = Quaternion.Euler(0f, 360f / _summonCount * i, 0f) * Vector3.forward * _summonRadius;
            if (!NavMesh.SamplePosition(transform.position + offset, out NavMeshHit hit, _summonRadius, NavMesh.AllAreas)) continue;
            MeleeEnemy minion = Instantiate(_summonPrefab, hit.position, Quaternion.identity);
            minion.SetTarget(_player);
            _minions.Add(minion);
            Summoned?.Invoke(minion);
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
