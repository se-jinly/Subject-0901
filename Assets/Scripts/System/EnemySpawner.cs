using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰")]
    [SerializeField] private Transform _playerTransform;
    [Tooltip("정렬 스폰 기준점이 없을 때 쓰는 랜덤 스폰 반경"), SerializeField] private float _radius = 10f;
    [SerializeField] private float _sampleRadius = 5f;
    [SerializeField] private int _maxSpawnTry = 10;

    [Header("정렬 스폰")]
    [Tooltip("북쪽 스폰 기준점 (부대 앞줄이 서는 위치). 비우면 랜덤 위치에 스폰"), SerializeField] private Transform _spawnAnchor;
    [Tooltip("한 줄에 배치할 수 있는 전체 폭"), SerializeField] private float _lineWidth = 21f;
    [Tooltip("부대·적 사이 간격"), SerializeField] private float _spawnGap = 1.5f;
    [Tooltip("줄 사이 간격 (남쪽으로)"), SerializeField] private float _rowDepth = 6f;

    private readonly List<Vector3> _linePoints = new();
    private int _nextPoint;
    private int _rowIndex;

    // 웨이브 시작 때 한 번 호출: 줄 위치를 처음(북쪽 기준점)으로 되돌림
    public void BeginWave()
    {
        _linePoints.Clear();
        _nextPoint = 0;
        _rowIndex = 0;
    }

    // 같은 종류 적 묶음의 자리를 계산: 한 줄에 다 안 들어가면 남쪽으로 다음 줄, 각 줄은 가운데 정렬
    public void BeginGroup(int count, float itemWidth)
    {
        if (_spawnAnchor == null || count <= 0) return;
        _linePoints.Clear();
        _nextPoint = 0;
        float pitch = itemWidth + _spawnGap;
        int perRow = Mathf.Max(1, Mathf.FloorToInt((_lineWidth + _spawnGap) / pitch));
        for (int placed = 0; placed < count; _rowIndex++)
        {
            int inRow = Mathf.Min(perRow, count - placed);
            for (int i = 0; i < inRow; i++)
            {
                float x = (i - (inRow - 1) * 0.5f) * pitch;
                _linePoints.Add(_spawnAnchor.position + new Vector3(x, 0f, -_rowIndex * _rowDepth));
            }
            placed += inRow;
        }
    }

    public Enemy Spawn(Enemy prefab)
    {
        if (!TryGetSpawnPoint(out Vector3 point)) return null;
        Enemy enemy = Instantiate(prefab, point, GetFacing(point));
        enemy.SetTarget(_playerTransform);
        return enemy;
    }

    // index/count: 같은 웨이브의 부대 중 몇 번째인지. 플레이어 주위를 부대 수만큼 나눠 각자 다른 방위를 맡김
    public Squad SpawnSquad(Squad prefab, int index, int count)
    {
        if (!TryGetSpawnPoint(out Vector3 point)) return null;
        Squad squad = Instantiate(prefab, point, GetFacing(point));
        Vector3 toSpawn = (_spawnAnchor != null ? _spawnAnchor.position : point) - _playerTransform.position;
        toSpawn.y = 0f;
        if (toSpawn.sqrMagnitude < 0.001f) toSpawn = Vector3.forward;
        float angle = (index - (count - 1) * 0.5f) * (360f / count);
        squad.Setup(_playerTransform, Quaternion.Euler(0f, angle, 0f) * toSpawn.normalized, index, count);
        return squad;
    }

    // 정렬 스폰이면 모두 같은 방향(기준점 → 플레이어)을 봐서 줄이 흐트러지지 않음
    private Quaternion GetFacing(Vector3 from)
    {
        Vector3 origin = _spawnAnchor != null ? _spawnAnchor.position : from;
        Vector3 dir = _playerTransform.position - origin;
        dir.y = 0f;
        return dir.sqrMagnitude > 0.001f ? Quaternion.LookRotation(dir) : Quaternion.identity;
    }

    private bool TryGetSpawnPoint(out Vector3 point)
    {
        if (_nextPoint < _linePoints.Count)
        {
            Vector3 planned = _linePoints[_nextPoint++];
            if (NavMesh.SamplePosition(planned, out NavMeshHit lineHit, _sampleRadius, NavMesh.AllAreas))
            {
                point = lineHit.position;
                return true;
            }
        }
        for (int i = 0; i < _maxSpawnTry; i++)
        {
            Vector3 randomPos = Random.insideUnitSphere * _radius;
            randomPos.y = 0f;
            randomPos += transform.position;
            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, _sampleRadius, NavMesh.AllAreas))
            {
                point = hit.position;
                return true;
            }
        }
        point = Vector3.zero;
        return false;
    }
}
