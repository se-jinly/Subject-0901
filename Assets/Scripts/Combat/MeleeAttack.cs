using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MeleeAttack : MonoBehaviour
{
    [Header("공격")]
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private int _attackDamage = 10;
    [SerializeField] private float _attackCooldown = 0.3f;
    [SerializeField] private float _attackOffsetRatio = 0.5f;
    private float _attackCooldownLeft;
    [SerializeField] private HitFeedback _hitFeedback = new();

    [Header("이펙트")]
    [Tooltip("공격할 때 앞쪽에 나오는 휘두름 이펙트. 비워두면 없음"), SerializeField] private GameObject _swingEffectPrefab;
    [Tooltip("콤보 순서별 휘두름 이펙트 (플레이어용). 비워두면 위의 휘두름 이펙트를 항상 사용"), SerializeField] private GameObject[] _comboSwingEffects;
    [Tooltip("맞은 대상 위치에 나오는 타격 이펙트. 비워두면 없음"), SerializeField] private GameObject _hitEffectPrefab;
    [Tooltip("휘두름 이펙트가 나오는 높이 (공격자 발 기준)"), SerializeField] private float _effectHeight = 1f;
    [Tooltip("이펙트가 사라지기까지 시간(초)"), SerializeField] private float _effectLifetime = 1f;

    [Header("사운드")]
    [SerializeField] private AudioClip _hitClip;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        TickTime();
    }

    public float Cooldown => _attackCooldown;
    // 선딜(예고)을 넣는 호출자용: 쿨타임이 끝났는지, 공격 범위 중심이 어디인지
    public bool IsReady => _attackCooldownLeft <= 0f;
    public Vector3 AttackCenter => transform.position + _attackOffsetRatio * _attackRange * transform.forward;

    // ignoreCooldown: 호출하는 쪽(플레이어)이 공격 간격을 직접 관리할 때 사용
    // comboIndex: 0 이상이면 콤보 순서에 맞는 휘두름 이펙트를 사용 (-1이면 기본 이펙트)
    public void Execute(bool ignoreCooldown = false, int comboIndex = -1)
    {
        if (!ignoreCooldown && _attackCooldownLeft > 0) return;
        int hitCount = 0;
        _attackCooldownLeft = _attackCooldown;
        Vector3 center = transform.position + _attackOffsetRatio * _attackRange * transform.forward;
        GameObject swingEffect = comboIndex >= 0 && _comboSwingEffects != null && _comboSwingEffects.Length > 0
            ? _comboSwingEffects[comboIndex % _comboSwingEffects.Length]
            : _swingEffectPrefab;
        SpawnEffect(swingEffect, transform.position + Vector3.up * _effectHeight, transform.rotation);
        Collider[] hits = Physics.OverlapSphere(center, _attackRange, _targetLayer);
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(_attackDamage, transform.forward);
                SpawnEffect(_hitEffectPrefab, hit.bounds.center, Quaternion.LookRotation(transform.forward));
                hitCount++;
            }
        }
        _hitFeedback.Play(hitCount);
        _audioSource.PlayOneShot(_hitClip);
    }
    public void AddDamage(float amount) => _attackDamage += Mathf.RoundToInt(amount);

    private void SpawnEffect(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return;
        Destroy(Instantiate(prefab, position, rotation), _effectLifetime);
    }

    private void TickTime()
    {
        _attackCooldownLeft = Mathf.Max(0f, _attackCooldownLeft - Time.deltaTime);
    }
}
