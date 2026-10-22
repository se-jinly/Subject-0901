using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(MeleeAttack))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(HitFlash))]
[RequireComponent(typeof(RangedAttack))]
[RequireComponent(typeof(AudioSource))]

public class Player : MonoBehaviour, IDamageable
{
    [Header("이동 및 회전")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _gravity = -10f;
    [SerializeField] private float _rotationSpeed = 720f;
    private Camera _cam;
    [SerializeField] private CameraFollowing _cameraFollowing;
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

    [Header("대시 연출")]
    [Tooltip("대시하는 동안 플레이어를 따라다니며 궤적을 남기는 피 파도 이펙트. 비워두면 연출 없음"), SerializeField] private GameObject _dashEffectPrefab;
    [Tooltip("이펙트가 나오는 월드 Y 높이 (바닥 위)"), SerializeField] private float _dashEffectHeight = 0.15f;
    [Tooltip("생성 후 이펙트가 완전히 지워지기까지 시간(초). 대시 시간 + 궤적이 남아 있는 시간보다 길어야 함"), SerializeField] private float _dashEffectLifetime = 2f;
    private GameObject _dashEffect;

    [Header("점프")]
    [SerializeField] private bool _isInCombat;
    [SerializeField] private float _jumpSpeed = 6f;


    [Header("넉백")]
    private Vector3 _externalForce;
    [SerializeField] private float _knockbackPower = 10f;
    [Tooltip("미끄러짐"), SerializeField] private float _decay = 10f;


    [Header("체력")]
    [SerializeField] private int _maxHp;
    private int _currentHp;

    [SerializeField] private float _dieWait = 1.5f;
    [Tooltip("HP가 0 이하가 된 뒤 실제로 죽기까지의 유예 시간(초). 그 안에 회복하면 생존"), SerializeField] private float _deathGrace = 0.1f;
    private bool _isDead = false;
    private bool _isDying;
    // 사망한 뒤에는 남아 있는 공격·스킬이 더 이상 효과를 내지 않도록 다른 스크립트가 확인함
    public bool IsDead => _isDead;
    [SerializeField] private HealthBar _healthBar;


    private Vector3 _camForward;
    private Vector3 _camRight;



    [Header("원거리 공격")]
    [Tooltip("꺼두면 원거리 공격 입력이 무시됨"), SerializeField] private bool _rangedEnabled = false;

    [Header("공격 연출")]
    [Tooltip("공격 애니메이션을 재생하기 시작하는 클립 시점(초). 준비 동작을 건너뛰어 반응이 빨라짐"), SerializeField] private float _attackStartTime = 0.5f;
    [Tooltip("공격 애니메이션이 끝나는 클립 시점(초). Animator의 Attack → Idle 전환 시점과 같아야 함"), SerializeField] private float _attackEndTime = 1.2f;
    [Tooltip("클립에서 칼이 휘둘러지기 시작하는 시점(초). 이 시점에 피해가 들어감"), SerializeField] private float _attackHitTime = 0.75f;
    private float _attackAnimSpeed = 1f;
    private float _attackReadyTime;

    private MeleeAttack _meleeAttack;
    private RangedAttack _rangedAttack;
    private InputAction _attackAction;
    private InputAction _rangedAttackAction;
    private HitFlash _hitFlash;
    private Animator _animator;

    public bool InputLocked { get; set; }

    [Header("사망 이펙트")]
    [Tooltip("죽은 자리 바닥에 나오는 이펙트. 비워두면 없음"), SerializeField] private GameObject _deathEffectPrefab;
    [Tooltip("이펙트가 나오는 월드 Y 높이 (바닥 위)"), SerializeField] private float _deathEffectHeight = 0.15f;
    [Tooltip("이펙트가 사라지기까지 시간(초)"), SerializeField] private float _deathEffectLifetime = 3f;

    [Header("오디오")]
    [SerializeField] private AudioClip _dashCilp;
    [SerializeField] private AudioClip _dieCilp;
    private AudioSource _audioSource;




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
        // 공격 쿨다운 안에 애니메이션이 끝나도록 재생 속도를 맞춤
        _attackAnimSpeed = (_attackEndTime - _attackStartTime) / Mathf.Max(0.05f, _meleeAttack.Cooldown);
        _animator.SetFloat("AttackSpeed", _attackAnimSpeed);
        _rangedAttack = GetComponent<RangedAttack>();

        _audioSource = GetComponent<AudioSource>();
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
        if (InputLocked) return;
        if (_isDead) return;
        _animator.SetBool("IsGround", _cc.isGrounded);
        _input = _moveAction.ReadValue<Vector2>();
        TickTimers();
        TryMobility();
        _animator.SetFloat("Speed", _input.magnitude);
        _animator.SetBool("IsDash", IsDashing);
        if (IsDashing) { Dash(); }
        else { EndDashEffect(); Rotate(); Move(); }
        TryAttack();
        if (_rangedEnabled) TryRangedAttack();
    }

    public void AddMaxHealth(float amount)
    {
        int value = Mathf.RoundToInt(amount);
        _maxHp += value;
        _currentHp += value;
        _healthBar.SetHealth(_currentHp, _maxHp);
    }
    public void Heal(float amount)
    {
        if (_isDead) return;
        int value = Mathf.RoundToInt(amount);
        _currentHp = Mathf.Min(_currentHp + value, _maxHp);
        _healthBar.SetHealth(_currentHp, _maxHp);
    }
    public void AddMoveSpeed(float amount) => _moveSpeed += amount;
    public void AddAttackDamage(float amount)
    {
        _meleeAttack.AddDamage(amount);
        _rangedAttack.AddDamage(amount);
    }
    public void MultiplyDashCooldown(float multiplier)
        => _dashCooldown = Mathf.Max(0.2f, _dashCooldown * multiplier);

    public void TakeDamage(int amount, Vector3 dir)
    {
        if (_isDead) return;
        _currentHp -= amount;
        _externalForce = dir * _knockbackPower;
        _healthBar.SetHealth(_currentHp, _maxHp);
        Debug.Log($"{gameObject}가, {amount}맞음, {_currentHp}남음");
        _hitFlash.Play();
        if (_currentHp <= 0 && !_isDying) StartCoroutine(DeathGraceRoutine());
    }

    // HP가 0 이하가 되어도 바로 죽지 않고 잠시 기다림: 그 사이 회복해서 HP가 남으면 생존
    private IEnumerator DeathGraceRoutine()
    {
        _isDying = true;
        yield return new WaitForSeconds(_deathGrace);
        _isDying = false;
        if (_currentHp <= 0) Die();
    }

    private void TryRangedAttack()
    {
        if (!_rangedAttackAction.WasPressedThisFrame()) return;
        _animator.SetTrigger("Throw");
        _rangedAttack.Execute();
    }

    private void Die()
    {
        _audioSource.PlayOneShot(_dieCilp);
        _isDead = true;
        _cc.enabled = false;
        _animator.SetTrigger("Die");
        SpawnDeathEffect();
        StartCoroutine(DieRoutine());
    }

    private void SpawnDeathEffect()
    {
        if (_deathEffectPrefab == null) return;
        Vector3 pos = new Vector3(transform.position.x, _deathEffectHeight, transform.position.z);
        Destroy(Instantiate(_deathEffectPrefab, pos, Quaternion.identity), _deathEffectLifetime);
    }

    private IEnumerator DieRoutine()
    {
        //Time.timeScale = 0f;
        gameObject.layer = LayerMask.NameToLayer("Default");
        yield return new WaitForSecondsRealtime(_dieWait);
        TimeControl.Reset();
        gameObject.layer = LayerMask.NameToLayer("Player");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void TryAttack()
    {
        if (!_attackAction.WasPressedThisFrame()) return;
        if (Time.time < _attackReadyTime) return;
        _attackReadyTime = Time.time + _meleeAttack.Cooldown;
        // 준비 동작을 건너뛰고 휘두르기 직전 시점부터 바로 재생
        // (고정 시간 오프셋은 배속이 적용된 상태 시간 기준이라 재생 속도로 나눠야 클립의 해당 시점이 됨)
        _animator.CrossFadeInFixedTime("Attack", 0.03f, 0, _attackStartTime / _attackAnimSpeed);
        StartCoroutine(AttackHitRoutine());
    }

    // 클릭 즉시가 아니라 애니메이션에서 칼이 휘둘러지기 시작하는 시점에 피해를 줌
    private IEnumerator AttackHitRoutine()
    {
        yield return new WaitForSeconds((_attackHitTime - _attackStartTime) / _attackAnimSpeed);
        if (_isDead) yield break;
        _meleeAttack.Execute(true);
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
            _audioSource.PlayOneShot(_dashCilp);
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
            SpawnDashEffect();
        }
    }

    private void SpawnDashEffect()
    {
        if (_dashEffectPrefab == null) return;
        _dashEffect = Instantiate(_dashEffectPrefab, DashEffectPosition(), Quaternion.LookRotation(_dashDirection));
        Destroy(_dashEffect, _dashEffectLifetime);
    }

    private Vector3 DashEffectPosition() => new Vector3(transform.position.x, _dashEffectHeight, transform.position.z);

    // 대시가 끝나면 새 파티클만 멈추고, 이미 나온 궤적은 스스로 사라질 때까지 둠
    private void EndDashEffect()
    {
        if (_dashEffect == null) return;
        foreach (ParticleSystem ps in _dashEffect.GetComponentsInChildren<ParticleSystem>())
            ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        _dashEffect = null;
    }
    private void Dash()
    {
        Vector3 dash = _dashDirection * _dashSpeed;
        dash.y = 0;
        _cc.Move(dash * Time.deltaTime);
        if (_dashEffect != null) _dashEffect.transform.position = DashEffectPosition();
    }
    // 대시 중엔 스킬 및 공격 사용 불가

    void Rotate() // 기본 회전은 전부 rotate에서
    {
        _groundPlane = new(Vector3.up, transform.position + Vector3.up * _aimPlaneHeight);
        Ray ray = _cameraFollowing.ScreenPointToRay(Mouse.current.position.ReadValue());
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