using UnityEngine;

public class RangedAttack : MonoBehaviour
{
    [Header("원거리 공격")]
    [SerializeField] private Projectile _projectilePrefab;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private int _attackDamage = 8;
    [SerializeField] private float _attackCoolDown = 0.5f;
    [SerializeField] private float _spawnOffset = 1f;
    [SerializeField] private float _spawnHeight = 1f;
    private float _attackCoolDownLeft;


    void Update()
    {
        TickTime();
    }

    public void Execute()
    {
        if (_attackCoolDownLeft > 0f) return;
        _attackCoolDownLeft = _attackCoolDown;
        Vector3 center = transform.position + transform.forward * _spawnOffset + transform.up * _spawnHeight;
        Projectile projectile = Instantiate(_projectilePrefab, center, transform.rotation);
        projectile.Setup(_attackDamage, _targetLayer);
    }

    private void TickTime()
    {
        _attackCoolDownLeft = Mathf.Max(0f, _attackCoolDownLeft - Time.deltaTime);
    }
}
