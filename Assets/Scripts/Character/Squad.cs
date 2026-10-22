using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Squad : MonoBehaviour
{
    [Header("구성")]
    [SerializeField] private MeleeEnemy _meleePrefab;
    [SerializeField] private RangedEnemy _rangedPrefab;
    [Tooltip("앞줄 근접 인원"), SerializeField] private int _meleeCount = 5;
    [Tooltip("뒷줄 원거리 인원"), SerializeField] private int _rangedCount = 5;
    [Tooltip("부대장 프리팹 (근접·원거리 중 랜덤으로 한 명이 나오고, 그 줄 인원에 포함됨)"), SerializeField] private MeleeEnemy _meleeLeaderPrefab;
    [SerializeField] private RangedEnemy _rangedLeaderPrefab;

    [Header("대형")]
    [Tooltip("같은 줄 좌우 간격"), SerializeField] private float _columnSpacing = 1.5f;
    [Tooltip("앞줄과 뒷줄 사이 간격"), SerializeField] private float _rowSpacing = 2.5f;
    [Tooltip("슬롯 위치를 NavMesh에서 찾는 반경"), SerializeField] private float _sampleRadius = 2f;

    [Header("이동")]
    [Tooltip("부대 이동 속도 (멤버보다 느리게)"), SerializeField] private float _moveSpeed = 1.5f;
    [Tooltip("포위 시 플레이어와의 최소 거리 (근접 사거리)"), SerializeField] private float _stopDistance = 2.5f;
    [Tooltip("포위 시 옆 부대와 비워둘 간격 (부대가 많으면 거리가 자동으로 늘어남)"), SerializeField] private float _ringGap = 2f;
    [Tooltip("부대 회전 속도 (낮을수록 대형이 천천히 돌아 후방이 노출됨)"), SerializeField] private float _rotateSpeed = 60f;

    [Header("대치")]
    [Tooltip("플레이어가 이 거리 이상 떨어진 채로 유지되면 대치로 봄"), SerializeField] private float _standoffDistance = 6f;
    [Tooltip("대치로 판단하기까지 걸리는 시간(초)"), SerializeField] private float _standoffTime = 2f;
    private float _standoffTimer;

    [Header("엄폐 대응")]
    [Tooltip("화살을 막는 장애물 레이어 (플레이어가 숨은 곳 판정)"), SerializeField] private LayerMask _sightMask;
    [Tooltip("시야 검사 높이"), SerializeField] private float _sightHeight = 1f;
    [Tooltip("우회 방위를 찾는 각도 간격(도)"), SerializeField] private float _flankStep = 10f;
    [Tooltip("우회 방위를 다시 계산하는 간격(초)"), SerializeField] private float _flankInterval = 0.25f;
    // 선호하는 쪽이 이만큼(도)까지 더 멀어도 선호하는 쪽으로 돌아감
    private const float FlankSideBias = 40f;
    private Vector3 _activeBearing;
    private float _flankTimer;
    private int _squadIndex;
    private int _squadCount;

    [Header("일제 사격")]
    [Tooltip("일제 사격 간격(초)"), SerializeField] private float _volleyInterval = 2f;
    [Tooltip("플레이어가 이 거리 안일 때만 사격"), SerializeField] private float _volleyRange = 10f;

    private readonly List<MeleeEnemy> _melees = new();
    private readonly List<RangedEnemy> _rangeds = new();
    private Enemy _leader;
    private NavMeshAgent _agent;
    private Transform _target;
    private float _volleyTimer;
    private Vector3 _bearing;
    private float _ringRadius;
    private bool _isSetup;

    // 부대 좌우 폭 (스폰 때 부대끼리 간격을 계산하는 데 사용)
    public float Width => (Mathf.Max(_meleeCount, _rangedCount) - 1) * _columnSpacing;

    // 대치 중에는 병사들이 대형을 벗어나지 않고 부대로 돌아감
    public bool IsStandoff => _standoffTimer >= _standoffTime;

    // 원거리가 없으면 대형을 풀고 근접이 곧장 돌진함
    public bool IsCharging => _isSetup && _rangeds.Count == 0;

    public IEnumerable<Enemy> Members
    {
        get
        {
            foreach (MeleeEnemy melee in _melees) yield return melee;
            foreach (RangedEnemy ranged in _rangeds) yield return ranged;
        }
    }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.speed = _moveSpeed;
        _agent.stoppingDistance = 0.3f;
        // 부대 기준점이 멤버들에게 막혀 멈추지 않도록
        _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
    }

    // bearing: 플레이어를 기준으로 이 부대가 맡을 방위, squadIndex/squadCount: 함께 포위하는 부대 중 몇 번째인지
    public void Setup(Transform target, Vector3 bearing, int squadIndex, int squadCount)
    {
        _target = target;
        _bearing = bearing.normalized;
        _activeBearing = _bearing;
        _squadIndex = squadIndex;
        _squadCount = squadCount;
        // 부대 앞줄이 둥글게 늘어설 때 서로 겹치지 않을 만큼 원이 커야 함
        float ringCircumference = squadCount * (Width + _ringGap);
        _ringRadius = Mathf.Max(_stopDistance, ringCircumference / (2f * Mathf.PI));
        _volleyTimer = _volleyInterval;
        bool isMeleeLeader = Random.value < 0.5f;
        MeleeEnemy meleeLeader = SpawnRow(_meleePrefab, isMeleeLeader ? _meleeLeaderPrefab : null, _melees, _meleeCount, 0f);
        RangedEnemy rangedLeader = SpawnRow(_rangedPrefab, isMeleeLeader ? null : _rangedLeaderPrefab, _rangeds, _rangedCount, _rowSpacing);
        _leader = meleeLeader != null ? meleeLeader : rangedLeader;
        _isSetup = true;
    }

    private void Update()
    {
        if (!_isSetup) return;
        _melees.RemoveAll(e => e == null);
        _rangeds.RemoveAll(e => e == null);
        if (_melees.Count == 0 && _rangeds.Count == 0)
        {
            Destroy(gameObject);
            return;
        }
        if (_target == null) return;
        Move();
        TickStandoff();
        TickVolley();
    }

    private void TickStandoff()
    {
        bool isFar = Vector3.Distance(transform.position, _target.position) >= _standoffDistance;
        _standoffTimer = isFar ? _standoffTimer + Time.deltaTime : 0f;
    }

    // 플레이어 주위의 자기 방위 지점으로 쉬지 않고 이동: 부대마다 방위가 달라 둥글게 포위함
    private void Move()
    {
        _flankTimer -= Time.deltaTime;
        if (_flankTimer <= 0f)
        {
            _flankTimer = _flankInterval;
            _activeBearing = FindClearBearing();
        }
        _agent.SetDestination(_target.position + _activeBearing * _ringRadius);
        Vector3 dir = _target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), _rotateSpeed * Time.deltaTime);
    }

    // 해당 방위의 포위 지점에서 플레이어까지 화살 경로가 장애물에 막혀 있는지
    private bool IsBlocked(Vector3 bearing)
    {
        Vector3 from = _target.position + bearing * _ringRadius + Vector3.up * _sightHeight;
        Vector3 to = _target.position + Vector3.up * _sightHeight;
        return Physics.Linecast(from, to, _sightMask);
    }

    // 막혀 있으면 방위를 돌려 가며 시야가 트이는 지점(장애물 가장자리 옆)을 찾음. 막혀 있지 않으면 원래 방위
    private Vector3 FindClearBearing()
    {
        if (IsCharging || !IsBlocked(_bearing)) return _bearing;
        float left = FindClearAngle(-1f);
        float right = FindClearAngle(1f);
        if (left == float.MaxValue && right == float.MaxValue) return _bearing;

        // 앞쪽 부대는 왼쪽, 뒤쪽 부대는 오른쪽을 선호해서 장애물 좌우로 나뉘어 막음 (너무 멀면 가까운 쪽)
        float mid = (_squadCount - 1) * 0.5f;
        float sign;
        if (_squadIndex < mid) sign = left <= right + FlankSideBias ? -1f : 1f;
        else if (_squadIndex > mid) sign = right <= left + FlankSideBias ? 1f : -1f;
        else sign = left <= right ? -1f : 1f;
        return Quaternion.Euler(0f, sign * (sign < 0f ? left : right), 0f) * _bearing;
    }

    private float FindClearAngle(float sign)
    {
        for (float angle = _flankStep; angle <= 180f; angle += _flankStep)
        {
            if (!IsBlocked(Quaternion.Euler(0f, sign * angle, 0f) * _bearing)) return angle;
        }
        return float.MaxValue;
    }

    // 부대 신호: 간격마다 원거리 전원이 동시에 발사
    private void TickVolley()
    {
        _volleyTimer -= Time.deltaTime;
        if (_volleyTimer > 0f) return;
        if (Vector3.Distance(transform.position, _target.position) > _volleyRange) return;
        _volleyTimer = _volleyInterval;
        foreach (RangedEnemy ranged in _rangeds) ranged.FireVolley();
    }

    // 살아있는 멤버끼리 줄을 다시 맞추기 때문에 죽은 자리는 자동으로 채워진다
    public Vector3 GetSlotPosition(Enemy member)
    {
        if (member is MeleeEnemy melee)
        {
            return GetRowPosition(GetSlotX(_melees, melee), 0f);
        }
        RangedEnemy ranged = (RangedEnemy)member;
        return GetRowPosition(GetSlotX(_rangeds, ranged), _rowSpacing);
    }

    // 부대장이 있는 줄은 부대장이 항상 한가운데, 나머지는 좌우로 번갈아 채움
    private float GetSlotX<T>(List<T> row, T member) where T : Enemy
    {
        int memberIndex = row.IndexOf(member);
        int leaderIndex = _leader == null ? -1 : row.IndexOf(_leader as T);
        if (leaderIndex < 0) return GetRegularX(memberIndex, row.Count);
        if (memberIndex == leaderIndex) return 0f;
        return GetOffCenterX(memberIndex > leaderIndex ? memberIndex - 1 : memberIndex);
    }

    private float GetRegularX(int index, int count) => (index - (count - 1) * 0.5f) * _columnSpacing;

    // 가운데(부대장)를 뺀 n번째 자리: 왼쪽 1칸, 오른쪽 1칸, 왼쪽 2칸 ...
    private float GetOffCenterX(int n)
    {
        int rank = n / 2 + 1;
        return (n % 2 == 0 ? -rank : rank) * _columnSpacing;
    }

    private Vector3 GetRowPosition(float x, float rowOffset)
    {
        return transform.TransformPoint(new Vector3(x, 0f, -rowOffset));
    }

    // leaderPrefab이 있으면 그 줄의 한가운데에 부대장 한 명을 세우고 나머지를 채움 (count에 부대장 포함)
    private T SpawnRow<T>(T prefab, T leaderPrefab, List<T> row, int count, float rowOffset) where T : Enemy
    {
        if (prefab == null) return null;
        bool hasLeader = leaderPrefab != null && count > 0;
        T leader = hasLeader ? SpawnMember(leaderPrefab, row, 0f, rowOffset) : null;
        int others = hasLeader ? count - 1 : count;
        for (int i = 0; i < others; i++)
        {
            float x = hasLeader ? GetOffCenterX(i) : GetRegularX(i, count);
            SpawnMember(prefab, row, x, rowOffset);
        }
        return leader;
    }

    private T SpawnMember<T>(T prefab, List<T> row, float x, float rowOffset) where T : Enemy
    {
        Vector3 slot = GetRowPosition(x, rowOffset);
        if (!NavMesh.SamplePosition(slot, out NavMeshHit hit, _sampleRadius, NavMesh.AllAreas)) return null;
        T member = Instantiate(prefab, hit.position, transform.rotation);
        member.SetTarget(_target);
        member.JoinSquad(this);
        row.Add(member);
        return member;
    }

    // 에디터에서 대형 확인용
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < _meleeCount; i++) Gizmos.DrawWireSphere(GetRowPosition(GetRegularX(i, _meleeCount), 0f), 0.4f);
        Gizmos.color = Color.cyan;
        for (int i = 0; i < _rangedCount; i++) Gizmos.DrawWireSphere(GetRowPosition(GetRegularX(i, _rangedCount), _rowSpacing), 0.4f);
    }
}
