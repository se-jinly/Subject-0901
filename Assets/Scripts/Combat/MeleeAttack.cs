using System.Collections;
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [Header("공격")]
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private int _attackDamage = 10;
    [SerializeField] private float _attackCooldown = 0.3f;
    [SerializeField] private float _attackOffsetRatio = 0.5f;
    [SerializeField] private float _shakePower = 0.1f;
    [SerializeField] private float _shakeDuration = 0.1f;
    [SerializeField] private float _hitStopDuration = 0.04f;
    private float _attackCooldownLeft;

    void Update()
    {
        TickTime();
    }

    public void Execute()
    {
        if (_attackCooldownLeft > 0) return;
        _attackCooldownLeft = _attackCooldown;
        Vector3 center = transform.position + _attackOffsetRatio * _attackRange * transform.forward;
        Collider[] hits = Physics.OverlapSphere(center, _attackRange, _targetLayer);
        bool hitAnything = false;
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(_attackDamage, transform.forward);
                hitAnything = true;
            }
        }
        if(hitAnything)
        {
            Camera.main.GetComponent<CameraFollowing>().Shake(_shakePower, _shakeDuration);
            StartCoroutine(HitStop());
        }
    }
    private IEnumerator HitStop()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(_hitStopDuration);
        Time.timeScale = 1f;
    }

    private void TickTime()
    {
        _attackCooldownLeft = Mathf.Max(0f, _attackCooldownLeft - Time.deltaTime);
    }
}
