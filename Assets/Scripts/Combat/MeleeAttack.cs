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

    // ignoreCooldown: 호출하는 쪽(플레이어)이 공격 간격을 직접 관리할 때 사용
    public void Execute(bool ignoreCooldown = false)
    {
        if (!ignoreCooldown && _attackCooldownLeft > 0) return;
        int hitCount = 0;
        _attackCooldownLeft = _attackCooldown;
        Vector3 center = transform.position + _attackOffsetRatio * _attackRange * transform.forward;
        Collider[] hits = Physics.OverlapSphere(center, _attackRange, _targetLayer);
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(_attackDamage, transform.forward);
                hitCount++;
            }
        }
        _hitFeedback.Play(hitCount);
        _audioSource.PlayOneShot(_hitClip);
    }
    public void AddDamage(float amount) => _attackDamage += Mathf.RoundToInt(amount);

    private void TickTime()
    {
        _attackCooldownLeft = Mathf.Max(0f, _attackCooldownLeft - Time.deltaTime);
    }
}
