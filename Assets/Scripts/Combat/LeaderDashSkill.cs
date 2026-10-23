using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// 부대장 스킬: 선딜 뒤 플레이어 쪽으로 돌진하고, 닿으면 한 번 피해를 줌
[RequireComponent(typeof(Enemy))]
public class LeaderDashSkill : MonoBehaviour
{
    [Header("돌진")]
    [SerializeField] private int _damage = 4;
    [SerializeField] private float _cooldown = 6f;
    [Tooltip("플레이어가 이 거리 안일 때만 발동"), SerializeField] private float _triggerRange = 6f;
    [Tooltip("돌진 전 멈춰서 조준하는 시간(초)"), SerializeField] private float _windupTime = 0.6f;
    [SerializeField] private float _dashSpeed = 12f;
    [SerializeField] private float _dashDuration = 0.5f;
    [Tooltip("돌진 중 플레이어가 이 거리 안이면 피해"), SerializeField] private float _hitRange = 2f;
    [Tooltip("선딜 중 플레이어를 바라보는 회전 속도"), SerializeField] private float _rotateSpeed = 360f;

    private Enemy _enemy;
    private NavMeshAgent _agent;
    private HitFlash _hitFlash;
    private float _cooldownLeft;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _agent = GetComponent<NavMeshAgent>();
        _hitFlash = GetComponent<HitFlash>();
        _cooldownLeft = _cooldown;
    }

    private void Update()
    {
        _cooldownLeft = Mathf.Max(0f, _cooldownLeft - Time.deltaTime);
        if (_enemy.IsActing || _enemy.Target == null || _cooldownLeft > 0f) return;
        if (Vector3.Distance(transform.position, _enemy.Target.position) > _triggerRange) return;
        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        _enemy.IsActing = true;
        _agent.ResetPath();
        _hitFlash.Blink(_windupTime);

        float t = 0f;
        while (t < _windupTime)
        {
            t += Time.deltaTime;
            FacePlayer();
            yield return null;
        }

        Vector3 dir = _enemy.Target.position - transform.position;
        dir.y = 0f;
        dir.Normalize();

        bool canHit = true;
        t = 0f;
        while (t < _dashDuration)
        {
            t += Time.deltaTime;
            _agent.Move(dir * _dashSpeed * Time.deltaTime);
            if (canHit && Vector3.Distance(_enemy.Target.position, transform.position) <= _hitRange
                && _enemy.Target.TryGetComponent(out IDamageable target))
            {
                target.TakeDamage(_damage, dir);
                canHit = false;
            }
            yield return null;
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
