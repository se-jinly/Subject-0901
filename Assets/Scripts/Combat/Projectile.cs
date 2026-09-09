using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("원거리")]
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _lifeTime = 3f;
    private int _damage;
    private LayerMask _targetLayer;
    [SerializeField] private LayerMask _blockLayer;
    public void Setup(int damage, LayerMask targetLayer)
    {
        _damage = damage;
        _targetLayer = targetLayer;
        Destroy(gameObject, _lifeTime);// 시한폭탄
    }

    void Update()
    {
        transform.position += _speed * Time.deltaTime * transform.forward;
    }

    private void OnTriggerEnter(Collider other)
    {
        int otherBit = 1 << other.gameObject.layer;
        // 여기 비트연산이 레이어는 int형으로 각 자리수를 저장하기 때문에 숫자비교가 아니라
        // 어느 자리에 위치해있냐가 정확히 구분 됨.
        if((_blockLayer & otherBit) != 0)
        {
            Destroy(gameObject);
            return;
        }
        if ((_targetLayer.value & otherBit) == 0) return;
        // 게임오브젝트의 레이어는 숫자 ex 8이면 1000상태라 그냥 레이어의 칸 만큼 옮기고
        // &연산으로 레이어랑 비교함.
        if (other.TryGetComponent<IDamageable>(out IDamageable target))
            // 여기 out은 그냥 변수 하나를 if앞에 만들었다고 보면 됨.
        {
            target.TakeDamage(_damage, transform.forward);
        }
        Destroy(gameObject);
    }

}

