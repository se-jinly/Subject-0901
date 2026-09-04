using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _radius = 10f;
    [SerializeField] private float _sampleRadius = 5f;
    [SerializeField] private int _maxSpawnTry = 10;

    public Enemy Spawn(Enemy prefab)
    {
        for (int i = 0; i < _maxSpawnTry; i++)
        {
            Vector3 randomPos = Random.insideUnitSphere * _radius;
            randomPos.y = 0f;
            randomPos += transform.position;
            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, _sampleRadius, NavMesh.AllAreas))
            {
                Enemy enemy = Instantiate(prefab, hit.position, Quaternion.identity);
                enemy.SetTarget(_playerTransform);
                return enemy;
            }
        }
        return null;
    }
}
