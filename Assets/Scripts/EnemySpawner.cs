using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰")]
    [SerializeField] private float _spawnWait = 5f;
    [Tooltip("플레이어 위치"), SerializeField] private Transform _playerTransform;
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private float _radius;
    
    IEnumerator EnemySpawnRoutine()
    {
        while(true)
        {
            Vector3 randomPos = Random.insideUnitSphere * _radius;
            randomPos.y = 0f;
            Enemy enemy = Instantiate(_enemyPrefab, randomPos, Quaternion.identity);
            enemy.SetTarget(_playerTransform);
            yield return new WaitForSeconds(_spawnWait);
        }
    }
    void Start()
    {
        StartCoroutine(EnemySpawnRoutine());
    }
}
