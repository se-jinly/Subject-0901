using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Wave
{
    public int meleeCount;
    public int rangedCount;
    public bool hasBoss;
}
// 이게 시러이라이저블이기 때문에 아마 안에 속한 모든게 정할 수 있을 것.
public class WaveManager : MonoBehaviour
{
    [Header("스폰 정보")]
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private MeleeEnemy _meleePrefab;
    [SerializeField] private RangedEnemy _rangedPrefab;
    [SerializeField] private BossEnemy _bossPrefab;
    [SerializeField] private Wave[] _waves;
    // 웨이브 정보만 있음 아마 숫자 많아야 십수개가 전부임
    [SerializeField] private float _waveInterval = 0.8f;
    // 간격
    private List<Enemy> _enemies = new();

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private GameObject _clearObject;
    private int _currentWave;
    [SerializeField] private RewardManager _rewardManager;
    [SerializeField] private Player _player;
    [SerializeField] private TextMeshProUGUI _resultText;

    private float _startTime;
    private int _killCount;
    void Start()
    {
        _startTime = Time.time;
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
        float elapsed = Time.time - _startTime;
        int minutes = (int)(elapsed / 60);
        int secends = (int)(elapsed % 60);
        _resultText.text = $"경과 시간: {minutes:00}:{secends:00}\n 처치 수: {_killCount}";
        // text받아서 넣어야함.
        _player.InputLocked = true;
    }
    private void SpawnWave(Wave wave)
    {
        for (int i = 0; i < wave.meleeCount; i++) SpawnEnemy(_meleePrefab);
        for (int i = 0; i < wave.rangedCount; i++) SpawnEnemy(_rangedPrefab);
        if (wave.hasBoss) SpawnEnemy(_bossPrefab);
    }
    private void SpawnEnemy(Enemy enemyPrefab)
    {
        Enemy enemy = _enemySpawner.Spawn(enemyPrefab);
        if (enemy != null)
        {
            _enemies.Add(enemy);
            _killCount++;
        }
    }

}
