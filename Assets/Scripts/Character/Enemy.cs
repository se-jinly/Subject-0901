using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(HitFlash))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public abstract class Enemy : MonoBehaviour, IDamageable
{
    //상속하는 것이라는 것 말곤 abstract가 제한되는 건 뭐임? 뭔가 구현하면 안 된다거나 그런거
    [Header("공격할 플레이어 찾기")]
    protected Transform _player;
    protected NavMeshAgent _agent;
    protected Animator _animator;
    private HitFlash _hitFlash;



    [SerializeField] protected float _detectRange = 3f;
    // 이제 nav쓰니까 이거 뭔가 어떻게 할 수 없나? 있을 것 같은데
    // 그리고 다른 건데 이거 코딩할 때 지키는 규칙들이 좀 있던데 내가 그걸 잘 모르겠네
    [SerializeField] private float _moveSpeed = 2f;


    [Header("체력")]
    [SerializeField] private int _maxHp = 10;
    private float _currentHp;


    [Header("넉백")]
    protected Vector3 _externalForce;
    [SerializeField] private float _knockbackPower = 10f;
    [Tooltip("미끄러짐"), SerializeField] private float _decay = 10f;


    protected virtual void Awake()
    {
        _currentHp = _maxHp;
        _hitFlash = GetComponent<HitFlash>();
        _animator = GetComponentInChildren<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _moveSpeed;
    }

    private void Update()
    {
        if (_player == null) return;
        _animator.SetFloat("Speed", _agent.velocity.magnitude);
        ApplyKnockback();
        TryAttack();
        Chase();
    }

    private void ApplyKnockback()
    {
        // 추적은 원거리랑 근거리가 다르니까, 그리고 밀쳐지는 것은 똑같으니까
        _agent.Move(_externalForce * Time.deltaTime);
        _externalForce = Vector3.Lerp(_externalForce, Vector3.zero, _decay * Time.deltaTime);
    }

    protected abstract void TryAttack();
    protected abstract void Chase();
    public void SetTarget(Transform target) => _player = target;

    public void TakeDamage(int amount, Vector3 dir)
    {
        _currentHp -= amount;
        _externalForce = dir * _knockbackPower;
        Debug.Log($"{name}, HP: {_currentHp}");
        if (_currentHp <= 0) Destroy(gameObject);
        _hitFlash.Play();
    }

}
