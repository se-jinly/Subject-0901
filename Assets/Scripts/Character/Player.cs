using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MeleeAttack))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(HitFlash))]
[RequireComponent(typeof(RangedAttack))]

public class Player : MonoBehaviour, IDamageable
{
    [Header("이동 및 회전")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _gravity = -10f;
    [SerializeField] private float _rotationSpeed = 720f;
    private Camera _cam;
    [SerializeField] private float _aimPlaneHeight = 1f;
    private Plane _groundPlane;
    private const float MinAimDistanceSqr = 0.01f;
    private float _verticalVelocity;
    [Tooltip("기본 중력"), SerializeField] private float _verticalBaseVelocity = -2f;
    private CharacterController _cc;
    private InputAction _moveAction;
    Vector2 _input;

    [Header("대시")]
    [SerializeField] private float _dashSpeed = 20f;
    [SerializeField] private float _dashDuration = 0.25f;
    [SerializeField] private float _dashCooldown = 1f;
    private InputAction _dashAction;
    private Vector3 _dashDirection;
    private float _dashTimeLeft;
    private float _cooldownLeft;
    private bool IsDashing => _dashTimeLeft > 0f;

    [Header("점프")]
    [SerializeField] private bool _isInCombat;
    [SerializeField] private float _jumpSpeed = 6f;

    private Vector3 _camForward;
    private Vector3 _camRight;



    private MeleeAttack _meleeAttack;
    private RangedAttack _rangedAttack;
    private InputAction _attackAction;
    private InputAction _rangedAttackAction;
    private HitFlash _hitFlash;


    [Header("넉백")]
    private Vector3 _externalForce;
    [SerializeField] private float _knockbackPower = 10f;
    [Tooltip("미끄러짐"), SerializeField] private float _decay = 10f;


    [Header("체력")]
    [SerializeField] private int _maxHp;
    private int _currentHp;

    [SerializeField] private float _dieWait = 1.5f;
    private bool _isDead = false;
    [SerializeField] private HealthBar _healthBar;
    private Animator _animator;





    void Awake()
    {
        _cam = Camera.main;

        _camForward = _cam.transform.forward;
        _camForward.y = 0;
        _camForward.Normalize();

        _camRight = _cam.transform.right;
        _camRight.y = 0;
        _camRight.Normalize();


        _cc = GetComponent<CharacterController>();
        _dashAction = InputSystem.actions.FindAction("Sprint");
        _moveAction = InputSystem.actions.FindAction("Move");
        _attackAction = InputSystem.actions.FindAction("Attack");
        _rangedAttackAction = InputSystem.actions.FindAction("RangedAttack");

        _currentHp = _maxHp;
        _meleeAttack = GetComponent<MeleeAttack>();
        _hitFlash = GetComponent<HitFlash>();
        _healthBar.SetHealth(_currentHp, _maxHp);
        _animator = GetComponentInChildren<Animator>();
        _rangedAttack = GetComponent<RangedAttack>();
    }

    void OnEnable()
    {
        _dashAction.Enable();
        _moveAction.Enable();
        _attackAction.Enable();
        _rangedAttackAction.Enable();
    }
    void OnDisable()
    {
        _dashAction.Disable();
        _moveAction.Disable();
        _attackAction.Disable();
        _rangedAttackAction.Disable();
    }

    void Update()
    {
        if (_isDead) return;
        _animator.SetBool("IsGround", _cc.isGrounded);
        _input = _moveAction.ReadValue<Vector2>();
        TickTimers();
        TryMobility();
        _animator.SetFloat("Speed", _input.magnitude);
        _animator.SetBool("IsDash", IsDashing);
        if (IsDashing) { Dash(); }
        else { Rotate(); Move(); }
        TryAttack();
        TryRangedAttack();
    }

    public void TakeDamage(int amount, Vector3 dir)
    {
        if (_isDead) return;
        _currentHp -= amount;
        _externalForce = dir * _knockbackPower;
        _healthBar.SetHealth(_currentHp, _maxHp);
        Debug.Log($"{gameObject}가, {amount}맞음, {_currentHp}남음");
        _hitFlash.Play();
        if (_currentHp <= 0) Die();
    }

    private void TryRangedAttack()
    {
        if (!_rangedAttackAction.WasPressedThisFrame()) return;
        _rangedAttack.Execute();
    }

    private void Die()
    {
        _isDead = true;
        _cc.enabled = false;
        _animator.SetTrigger("Die");
        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        //Time.timeScale = 0f;
        gameObject.layer = LayerMask.NameToLayer("Default");
        yield return new WaitForSecondsRealtime(_dieWait);
        Time.timeScale = 1f;
        gameObject.layer = LayerMask.NameToLayer("Player");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void TryAttack()
    {
        if (!_attackAction.WasPressedThisFrame()) return;
        _animator.SetTrigger("Attack");
        _meleeAttack.Execute();
    }

    private void TryMobility()
    {
        if(!_dashAction.WasPressedThisFrame()) return;
        if (_isInCombat) TryStartDash();
        else Jump();
    }

    private void Jump()
    {
        if(_cc.isGrounded)
        {
            _verticalVelocity = _jumpSpeed;
        }
    }

    private Vector3 InputMoveDirection()
    {
        return _input.x * _camRight + _input.y * _camForward;
    }

    private void TickTimers()
    {
        _dashTimeLeft = Mathf.Max(0f, _dashTimeLeft - Time.deltaTime);
        _cooldownLeft = Mathf.Max(0f, _cooldownLeft - Time.deltaTime);
    }
    private void TryStartDash()
    {
        if(!IsDashing && _cooldownLeft <= 0)
        {
            // _input의 값이 뭐라도 들어있으면 0이 아님
            if (_input.sqrMagnitude > MinAimDistanceSqr)
            {
                Vector3 dir = InputMoveDirection();
                _dashDirection = dir.normalized;
            } else
            {
                _dashDirection = transform.forward;
            }
            _dashTimeLeft = _dashDuration;
            _cooldownLeft = _dashCooldown;
        }
    }
    private void Dash()
    {
        Vector3 dash = _dashDirection * _dashSpeed;
        dash.y = 0;
        _cc.Move(dash * Time.deltaTime);
    }
    // 대시 중엔 스킬 및 공격 사용 불가

    void Rotate() // 기본 회전은 전부 rotate에서
    {
        _groundPlane = new(Vector3.up, transform.position + Vector3.up * _aimPlaneHeight);
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Ray ray = _cam.ScreenPointToRay(mouseScreenPos);
        if (_groundPlane.Raycast(ray, out float dist))
        {
            Vector3 hitPoint = ray.GetPoint(dist);
            Debug.DrawLine(transform.position, hitPoint, Color.red);
            Vector3 dir = hitPoint - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude <= MinAimDistanceSqr)
            {
                return;
            }
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
        }
    }
    // 회전은 이게 다인가? 상 하체 분리회전, 스킬 쓸 때 회전, 회전 공격, 모션(x축으로 굴러서)타격
    void Move() // 기본 움직임은 전부 move에서
    {
        Vector3 move = InputMoveDirection() * _moveSpeed;
        if (_cc.isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = _verticalBaseVelocity;
        }
        _verticalVelocity += _gravity * Time.deltaTime;
        move.y = _verticalVelocity;

        move += _externalForce;
        _cc.Move(move * Time.deltaTime);
        _externalForce = Vector3.Lerp(_externalForce, Vector3.zero, _decay * Time.deltaTime);
    }
    // 앵간해서 move를 더 할게 있나? 이속 버프, 감소, 정지, 공격하면서 이동?
}