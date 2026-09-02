using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeleeAttack))]
[RequireComponent(typeof(HitFlash))]
public class Enemy : MonoBehaviour, IDamageable
{
    [Header("공격할 플레이어 찾기")]
    private MeleeAttack _meleeAttack;
    private Transform _player;
    [SerializeField] private float _detectRange = 3f;
    [SerializeField] private float _moveSpeed = 2f;


    [Header("체력")]
    [SerializeField] private float _maxHp = 10f;
    private float _currentHp;

    private HitFlash _hitFlash;

    [Header("넉백")]
    private Vector3 _externalForce;
    [SerializeField] private float _knockbackPower = 10f;
    [Tooltip("미끄러짐"), SerializeField] private float _decay = 10f;


    private void Awake()
    {
        _meleeAttack = GetComponent<MeleeAttack>();
        _currentHp = _maxHp;
        _hitFlash = GetComponent<HitFlash>();
    }

    private void Update()
    {
        TryAttack();
        Chase();
    }

    private void TryAttack()
    {
        if (Vector3.Distance(transform.position, _player.position) > _detectRange) return;
        _meleeAttack.Execute();
    }
    private void Chase()
    {
        Vector3 move = Vector3.zero;
        if (Vector3.Distance(transform.position, _player.position) > 1f)
        {
            Vector3 dir = _player.position - transform.position;
            dir.y = 0f;
            move = dir.normalized * _moveSpeed;
        }
        move += _externalForce;
        transform.position += move * Time.deltaTime;
        _externalForce = Vector3.Lerp(_externalForce, Vector3.zero, _decay * Time.deltaTime);

    }
    public void SetTarget(Transform target) => _player = target;


    public void TakeDamage(float amount, Vector3 dir)
    {
        _currentHp -= amount;
        _externalForce = dir * _knockbackPower;
        Debug.Log($"{name}, HP: {_currentHp}");
        if (_currentHp <= 0) Destroy(gameObject);
        _hitFlash.Play();
    }

}