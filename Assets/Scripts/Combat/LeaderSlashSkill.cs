using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 부대장 스킬: 선딜 뒤 정면 부채꼴 범위를 베어 넘김 (SlashSkill의 적 버전)
[RequireComponent(typeof(Enemy))]
public class LeaderSlashSkill : MonoBehaviour
{
    [Header("횡베기")]
    [SerializeField] private int _damage = 3;
    [SerializeField] private float _cooldown = 5f;
    [Tooltip("플레이어가 이 거리 안일 때만 발동"), SerializeField] private float _triggerRange = 3f;
    [Tooltip("베는 범위 반경"), SerializeField] private float _range = 3f;
    [Tooltip("정면 기준 부채꼴 전체 각도(도)"), SerializeField] private float _arcAngle = 180f;
    [SerializeField] private LayerMask _targetLayer;
    [Tooltip("밀쳐내는 세기"), SerializeField] private float _knockback = 3f;
    [Tooltip("휘두르기 전 멈춰서 조준하는 시간(초)"), SerializeField] private float _windupTime = 0.5f;
    [Tooltip("선딜 중 플레이어를 바라보는 회전 속도"), SerializeField] private float _rotateSpeed = 360f;

    [Header("연출")]
    [Tooltip("선딜 동안 베는 범위(부채꼴)를 보여 주는 바닥 예고선. 비워두면 없음"), SerializeField] private GameObject _warningEffect;
    [SerializeField] private GameObject _effectPrefab;
    [Tooltip("이펙트가 원래 맞춰진 반경 (범위에 맞춰 이펙트 크기를 조절)"), SerializeField] private float _effectBaseRange = 4.5f;
    [Tooltip("이펙트가 나오는 월드 Y 높이 (바닥 위)"), SerializeField] private float _effectHeight = 0.15f;
    [Tooltip("이펙트가 사라지기까지 시간(초)"), SerializeField] private float _effectLifetime = 1.5f;

    private Enemy _enemy;
    private NavMeshAgent _agent;
    private Animator _animator;
    private HitFlash _hitFlash;
    private float _cooldownLeft;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _hitFlash = GetComponent<HitFlash>();
        _cooldownLeft = _cooldown;
        // 돌진 스킬도 있으면 둘 중 하나만 랜덤으로 사용 (꺼진 컴포넌트는 Update가 돌지 않음)
        if (TryGetComponent(out LeaderDashSkill dash))
        {
            bool useDash = Random.value < 0.5f;
            dash.enabled = useDash;
            enabled = !useDash;
        }
    }

    private void Update()
    {
        _cooldownLeft = Mathf.Max(0f, _cooldownLeft - Time.deltaTime);
        if (_enemy.IsActing || _enemy.Target == null || _cooldownLeft > 0f) return;
        if (Vector3.Distance(transform.position, _enemy.Target.position) > _triggerRange) return;
        StartCoroutine(SlashRoutine());
    }

    private IEnumerator SlashRoutine()
    {
        _enemy.IsActing = true;
        _agent.ResetPath();
        _animator.SetTrigger("Attack");
        _hitFlash.Blink(_windupTime);
        // 예고선은 몸에 붙어 플레이어를 바라보는 방향으로 같이 돎
        GameObject warning = _warningEffect != null
            ? Instantiate(_warningEffect, new Vector3(transform.position.x, _effectHeight, transform.position.z), transform.rotation, transform)
            : null;

        float t = 0f;
        while (t < _windupTime)
        {
            t += Time.deltaTime;
            FacePlayer();
            yield return null;
        }
        if (warning != null) Destroy(warning);

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        if (_effectPrefab != null)
        {
            Vector3 pos = new Vector3(transform.position.x, _effectHeight, transform.position.z);
            GameObject effect = Instantiate(_effectPrefab, pos, Quaternion.LookRotation(forward));
            effect.transform.localScale = _effectPrefab.transform.localScale * (_range / _effectBaseRange);
            Destroy(effect, _effectLifetime);
        }

        HashSet<IDamageable> hitTargets = new();
        foreach (Collider hit in Physics.OverlapSphere(transform.position, _range, _targetLayer))
        {
            Vector3 toTarget = hit.transform.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.001f && Vector3.Angle(forward, toTarget) > _arcAngle * 0.5f) continue;
            if (!hit.TryGetComponent(out IDamageable target) || !hitTargets.Add(target)) continue;
            Vector3 dir = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : forward;
            target.TakeDamage(_damage, dir * _knockback);
        }

        _cooldownLeft = _cooldown;
        _enemy.IsActing = false;
    }

    private void FacePlayer()
    {
        Vector3 dir = _enemy.Target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), _rotateSpeed * Time.deltaTime);
    }
}
