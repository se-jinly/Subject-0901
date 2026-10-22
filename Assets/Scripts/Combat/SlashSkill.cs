using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 시험용 능력: E를 누르면 정면 부채꼴 범위를 강하게 베어 넘김. 안 쓰기로 하면 Player에서 이 컴포넌트만 빼면 됨
public class SlashSkill : MonoBehaviour
{
    [Header("능력")]
    [SerializeField] private int _damage = 8;
    [Tooltip("베는 범위 반경"), SerializeField] private float _range = 4.5f;
    [Tooltip("정면 기준 부채꼴 전체 각도(도)"), SerializeField] private float _arcAngle = 180f;
    [SerializeField] private float _cooldown = 10f;
    [SerializeField] private LayerMask _targetLayer;
    [Tooltip("밀쳐내는 세기 배율 (적의 기본 넉백에 곱함)"), SerializeField] private float _knockbackScale = 3f;
    [Tooltip("누른 뒤 실제로 베이기까지 시간(초). 애니메이션의 휘두르는 시점에 맞춤"), SerializeField] private float _hitDelay = 0.35f;

    [Header("연출")]
    [SerializeField] private GameObject _effectPrefab;
    [Tooltip("이펙트가 나오는 월드 Y 높이 (바닥 위)"), SerializeField] private float _effectHeight = 0.15f;
    [Tooltip("이펙트가 사라지기까지 시간(초)"), SerializeField] private float _effectLifetime = 1.5f;
    [SerializeField] private float _shakePower = 0.35f;
    [SerializeField] private float _shakeDuration = 0.3f;
    [SerializeField] private float _hitStopDuration = 0.06f;

    private InputAction _skillAction;
    private Player _player;
    private Animator _animator;
    private float _cooldownLeft;

    // 쿨타임 UI용: 남은 시간(초)
    public float CooldownLeft => _cooldownLeft;

    // 쿨타임 UI용: 1이면 막 쓴 직후, 0이면 사용 가능
    public float CooldownRatio => _cooldown <= 0f ? 0f : Mathf.Clamp01(_cooldownLeft / _cooldown);

    private void Awake()
    {
        _player = GetComponent<Player>();
        _animator = GetComponentInChildren<Animator>();
        _skillAction = new InputAction("SlashSkill", InputActionType.Button, "<Keyboard>/e");
    }

    private void OnEnable() => _skillAction.Enable();
    private void OnDisable() => _skillAction.Disable();
    private void OnDestroy() => _skillAction.Dispose();

    private void Update()
    {
        _cooldownLeft = Mathf.Max(0f, _cooldownLeft - Time.deltaTime);
        if (_player.InputLocked || _player.IsDead) return;
        if (!_skillAction.WasPressedThisFrame()) return;
        if (_cooldownLeft > 0f) return;
        _cooldownLeft = _cooldown;
        _animator.SetTrigger("Attack");
        StartCoroutine(SlashRoutine());
    }

    private IEnumerator SlashRoutine()
    {
        yield return new WaitForSeconds(_hitDelay);
        // 그 사이 죽었으면 베기가 취소됨 (죽은 뒤에 적을 처치해 회복하는 일이 없도록)
        if (_player.IsDead) yield break;

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        if (_effectPrefab != null)
        {
            Vector3 pos = new Vector3(transform.position.x, _effectHeight, transform.position.z);
            Destroy(Instantiate(_effectPrefab, pos, Quaternion.LookRotation(forward)), _effectLifetime);
        }

        int hitCount = 0;
        HashSet<IDamageable> hitTargets = new();
        foreach (Collider hit in Physics.OverlapSphere(transform.position, _range, _targetLayer))
        {
            Vector3 toTarget = hit.transform.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.001f && Vector3.Angle(forward, toTarget) > _arcAngle * 0.5f) continue;
            if (!hit.TryGetComponent(out IDamageable target) || !hitTargets.Add(target)) continue;
            Vector3 dir = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : forward;
            target.TakeDamage(_damage, dir * _knockbackScale);
            hitCount++;
        }
        HitEffect.Instance.Execute(_shakePower, _shakeDuration, hitCount > 0 ? _hitStopDuration : 0f);
    }
}
