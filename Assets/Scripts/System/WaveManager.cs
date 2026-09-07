using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Wave
{
    public int meleeCount;
    public int rangedCount;
}
// 이게 시러이라이저블이기 때문에 아마 안에 속한 모든게 정할 수 있을 것.
public class WaveManager : MonoBehaviour
{
    [Header("스폰 정보")]
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private MeleeEnemy _meleePrefab;
    [SerializeField] private RangedEnemy _rangedPrefab;
    [SerializeField] private Wave[] _waves;
    // 웨이브 정보만 있음 아마 숫자 많아야 십수개가 전부임
    [SerializeField] private float _waveInterval = 3f;
    // 간격
    private List<Enemy> _enemies = new();

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private GameObject _clearObject;
    private int _currentWave;
    [SerializeField] private RewardManager _rewardManager;
    [SerializeField] private Player _player;
    void Start()
    {
        StartCoroutine(WaveRoutine());
    }
    private IEnumerator WaveRoutine()
    {
        for (int i = 0; i < _waves.Length; i++)
        {
            _currentWave = i;
            _waveText.text = $"{_currentWave +1} / {_waves.Length}";
            // 마찬가지 딱 봐도 웨이브랭스 넘겨서 그거 분의 이거 로 표시할 듯
            yield return new WaitForSeconds(_waveInterval);
            SpawnWave(_waves[i]);
            yield return new WaitUntil(() =>
            {
                _enemies.RemoveAll(e => e == null);
                return _enemies.Count == 0;
            });
            if (i + 1 < _waves.Length)
            {
                yield return StartCoroutine(_rewardManager.ShowAndWait());
            }
        }
        _clearObject.SetActive(true);
        _player.InputLocked = true;
    }
    private void SpawnWave(Wave wave)
    {
        for (int i = 0; i < wave.meleeCount; i++)
        {
            Enemy enemy = _enemySpawner.Spawn(_meleePrefab);
            if (enemy != null) _enemies.Add(enemy);
        }
        for (int i = 0; i < wave.rangedCount; i++)
        {
            Enemy enemy = _enemySpawner.Spawn(_rangedPrefab);
            if (enemy != null) _enemies.Add(enemy);
        }
    }

}
