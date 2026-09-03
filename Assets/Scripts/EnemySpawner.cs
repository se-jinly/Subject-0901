
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private float _spawnWait = 5f;
    [SerializeField] private float _radius = 15f;
    [SerializeField] private int _maxEnemyCount = 5;
    private List<Enemy> _enemies = new();

    void Start()
    {

        StartCoroutine(EnemySpawnRoutine());
    }
    IEnumerator EnemySpawnRoutine()
    {
        Spawn();
        while (true)
        {
            yield return new WaitForSeconds(_spawnWait);
            _enemies.RemoveAll(e => e == null);
            if (_enemies.Count >= _maxEnemyCount) continue;
            Spawn();
        }
    }

    private void Spawn()
    {
        Vector3 randomPos = Random.insideUnitSphere * _radius;
        randomPos.y = 0f;
        Enemy enemy = Instantiate(_enemyPrefab, _playerTransform.position + randomPos, Quaternion.identity);
        enemy.SetTarget(_playerTransform);
        _enemies.Add(enemy);
    }
}
