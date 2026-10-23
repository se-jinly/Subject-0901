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
    [Header("이펙트")]
    [Tooltip("돌진·소환 전 보스 발밑에 빛이 모이는 이펙트. 비워두면 없음"), SerializeField] private GameObject _windupEffect;
    [Tooltip("돌진하는 동안 보스를 따라다니며 꼬리를 남기는 이펙트"), SerializeField] private GameObject _dashTrailEffect;
    [Tooltip("돌진이 플레이어에게 닿는 위치에 나오는 이펙트"), SerializeField] private GameObject _dashImpactEffect;
    [Tooltip("졸개가 나타나는 위치마다 나오는 이펙트"), SerializeField] private GameObject _summonEffect;
    [Tooltip("돌진 전 돌진할 길을 보여 주는 예고선 (길이는 돌진 속도 × 돌진 시간에 맞춰 만든 프리팹). 비워두면 없음"), SerializeField] private GameObject _dashWarningEffect;
    [Tooltip("바닥 이펙트가 나오는 월드 Y 높이 (바닥 위)"), SerializeField] private float _effectHeight = 0.15f;
    [Tooltip("이펙트가 사라지기까지 시간(초)"), SerializeField] private float _effectLifetime = 2f;
    private readonly List<Enemy> _minions = new();
    // 소환된 졸개를 웨이브가 알 수 있도록 알림
    public event System.Action<Enemy> Summoned;


    private MeleeAttack _meleeAttack;
    private HitFlash _warnFlash;

    protected override void Awake()
    {
        base.Awake();
        _patternTimer = _patternInterval;
        _meleeAttack = GetComponent<MeleeAttack>();
        _warnFlash = GetComponent<HitFlash>();
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
        SpawnEffect(_windupEffect, transform.position);
        if(randomPattern == 0)
        {
            // 예고 시작 때 돌진 방향을 정함: 예고선과 실제 돌진 방향이 같고, 예고 동안 옆으로 피할 수 있음
            Vector3 dashDir = DashDirection();
            transform.rotation = Quaternion.LookRotation(dashDir);
            SpawnEffect(_dashWarningEffect, transform.position, transform.rotation);
            _warnFlash.Blink(_dashWindupTime);
            yield return new WaitForSeconds(_dashWindupTime);
            yield return StartCoroutine(DashRoutine(dashDir));
        } else
        {
            _warnFlash.Blink(_summonWindupTime);
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
            SpawnEffect(_summonEffect, hit.position);
            MeleeEnemy minion = Instantiate(_summonPrefab, hit.position, Quaternion.identity);
            minion.SetTarget(_player);
            _minions.Add(minion);
            Summoned?.Invoke(minion);
        }
        yield return null;
    }

    private Vector3 DashDirection()
    {
        Vector3 dir = _player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) return transform.forward;
        return dir.normalized;
    }

    private IEnumerator DashRoutine(Vector3 dir)
    {
        bool canHit = true;
        float t = 0f;
        // 돌진하는 동안 보스를 따라다니는 꼬리 이펙트
        GameObject trail = _dashTrailEffect != null ? Instantiate(_dashTrailEffect, transform) : null;

        while (t < _dashDuration)
        {
            t += Time.deltaTime;
            _agent.Move(dir * _dashSpeed * Time.deltaTime);

            if (canHit && Vector3.Distance(_player.position, transform.position) <= _dashRange)
            {
                _player.TryGetComponent<IDamageable>(out IDamageable target);
                target.TakeDamage(_dashDamage, dir);
                SpawnEffect(_dashImpactEffect, _player.position);
                canHit = false;
            }

            yield return null;
        }

        // 새 입자만 멈추고, 이미 나온 꼬리는 스스로 사라질 때까지 둠
        if (trail != null)
        {
            foreach (ParticleSystem ps in trail.GetComponentsInChildren<ParticleSystem>())
                ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            Destroy(trail, _effectLifetime);
        }
    }

    private void SpawnEffect(GameObject prefab, Vector3 position) => SpawnEffect(prefab, position, Quaternion.identity);

    private void SpawnEffect(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return;
        Vector3 pos = new Vector3(position.x, _effectHeight, position.z);
        Destroy(Instantiate(prefab, pos, rotation), _effectLifetime);
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
