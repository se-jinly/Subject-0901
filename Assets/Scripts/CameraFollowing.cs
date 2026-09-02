using UnityEngine;

public class CameraFollowing : MonoBehaviour
{
    // 카메라 이동(맵 끝)제한 및 궁극기 쓸 때나 타격감 이동 및 각성 등의 상태이상 변화.
    [Header("플레이어")]
    [SerializeField] private Transform _target;

    [Header("카메라")]
    [SerializeField] private float _followingSmooth = 5f;
    [Tooltip("얼마나 떨어져 있을 지"), SerializeField] private Vector3 _offset = new(0f, 15f, -10f);

    void LateUpdate()
    {
        if (_target == null) return;
        Vector3 targetPos = _target.position + _offset;
        Vector3 startPos = transform.position;
        transform.position = Vector3.Lerp(startPos, targetPos, _followingSmooth * Time.deltaTime);
    }
}
