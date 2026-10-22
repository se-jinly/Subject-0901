using UnityEngine;
using UnityEngine.InputSystem;

// 시험용 능력: Q를 누르면 코인을 쓰고 모든 적에게 피해. 안 쓰기로 하면 Player에서 이 컴포넌트만 빼면 됨
public class BloodSkill : MonoBehaviour
{
    [Header("능력")]
    [SerializeField] private int _damage = 10;
    [SerializeField] private int _coinCost = 1000;
    [Tooltip("연타 방지 대기 시간(초)"), SerializeField] private float _cooldown = 0.5f;

    [Header("연출")]
    [SerializeField] private GameObject _effectPrefab;
    [Tooltip("이펙트가 나오는 월드 Y 높이 (바닥 위)"), SerializeField] private float _effectHeight = 0.15f;
    [Tooltip("이펙트가 사라지기까지 시간(초)"), SerializeField] private float _effectLifetime = 3f;
    [SerializeField] private float _shakePower = 0.5f;
    [SerializeField] private float _shakeDuration = 0.6f;
    [SerializeField] private float _hitStopDuration = 0.08f;

    private InputAction _skillAction;
    private Player _player;
    private float _cooldownLeft;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _skillAction = new InputAction("BloodSkill", InputActionType.Button, "<Keyboard>/q");
    }

    private void OnEnable() => _skillAction.Enable();
    private void OnDisable() => _skillAction.Disable();
    private void OnDestroy() => _skillAction.Dispose();

    private void Update()
    {
        _cooldownLeft -= Time.deltaTime;
        if (_player.InputLocked || _player.IsDead) return;
        if (!_skillAction.WasPressedThisFrame()) return;
        if (_cooldownLeft > 0f) return;
        if (CoinUI.Instance == null || !CoinUI.Instance.TrySpend(_coinCost)) return;
        Cast();
    }

    private void Cast()
    {
        _cooldownLeft = _cooldown;
        if (_effectPrefab != null)
        {
            Vector3 pos = new Vector3(transform.position.x, _effectHeight, transform.position.z);
            Destroy(Instantiate(_effectPrefab, pos, Quaternion.identity), _effectLifetime);
        }
        HitEffect.Instance.Execute(_shakePower, _shakeDuration, _hitStopDuration);

        foreach (Enemy enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            Vector3 dir = enemy.transform.position - transform.position;
            dir.y = 0f;
            enemy.TakeDamage(_damage, dir.sqrMagnitude > 0.001f ? dir.normalized : Vector3.forward);
        }
    }
}
