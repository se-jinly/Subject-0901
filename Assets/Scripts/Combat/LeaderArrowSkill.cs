using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// 부대장 스킬: 멈춰서 조준한 뒤 거대한 관통 화살을 한 발 쏨
[RequireComponent(typeof(Enemy))]
public class LeaderArrowSkill : MonoBehaviour
{
    [Header("거대 화살")]
    [SerializeField] private Projectile _arrowPrefab;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private int _damage = 6;
    [SerializeField] private float _cooldown = 7f;
    [Tooltip("플레이어가 이 거리 안일 때만 발동"), SerializeField] private float _triggerRange = 12f;
    [Tooltip("쏘기 전 멈춰서 조준하는 시간(초)"), SerializeField] private float _windupTime = 0.8f;
    [Tooltip("조준 중 플레이어를 바라보는 회전 속도"), SerializeField] private float _rotateSpeed = 360f;
    [Tooltip("쏜 뒤 가만히 있는 시간(초)"), SerializeField] private float _recoveryTime = 0.4f;
    [SerializeField] private float _spawnOffset = 1.5f;
    [SerializeField] private float _spawnHeight = 1f;

    private Enemy _enemy;
    private NavMeshAgent _agent;
    private float _cooldownLeft;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _agent = GetComponent<NavMeshAgent>();
        _cooldownLeft = _cooldown;
    }

    private void Update()
    {
        _cooldownLeft = Mathf.Max(0f, _cooldownLeft - Time.deltaTime);
        if (_enemy.IsActing || _enemy.Target == null || _cooldownLeft > 0f) return;
        if (Vector3.Distance(transform.position, _enemy.Target.position) > _triggerRange) return;
        StartCoroutine(ArrowRoutine());
    }

    private IEnumerator ArrowRoutine()
    {
        _enemy.IsActing = true;
        _agent.ResetPath();

        float t = 0f;
        while (t < _windupTime)
        {
            t += Time.deltaTime;
            FacePlayer();
            yield return null;
        }

        Vector3 spawnPos = transform.position + transform.forward * _spawnOffset + Vector3.up * _spawnHeight;
        Projectile arrow = Instantiate(_arrowPrefab, spawnPos, transform.rotation);
        arrow.Setup(_damage, _targetLayer);

        yield return new WaitForSeconds(_recoveryTime);
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
