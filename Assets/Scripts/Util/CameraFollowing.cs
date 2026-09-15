using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CameraFollowing : MonoBehaviour
{
    // 카메라 이동(맵 끝)제한 및 궁극기 쓸 때나 타격감 이동 및 각성 등의 상태이상 변화.
    [Header("플레이어")]
    [SerializeField] private Transform _target;

    [Header("카메라")]
    [SerializeField] private float _followingSmooth = 5f;
    [SerializeField] private Vector3 _offset = new(0f, 15f, -10f);
    [Header("픽셀 스냅")]
    [SerializeField] private Camera _cam;
    [SerializeField] private int _rtHeight = 270; //480 * 270
    private Vector3 _snapPos; // 부드러운 말고 좀 정확한 픽셀이동 이런식으로 해야겠는데
    private Vector3 _shakeOffset;
    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        _snapPos = transform.position;
    }

    public void Shake(float power, float duration)
    {
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeRoutine(power, duration));
    }

    private IEnumerator ShakeRoutine(float power, float duration)
    {
        float elapsed = 0f;
        while(elapsed < duration)
        {
            _shakeOffset = Random.insideUnitSphere * power;
            elapsed += Time.deltaTime;
            yield return null;
        }
        _shakeOffset = Vector3.zero;
    }

    void LateUpdate()
    {
        if (_target == null) return;
        Vector3 targetPos = _target.position + _offset;
        _snapPos = Vector3.Lerp(_snapPos, targetPos, _followingSmooth * Time.deltaTime);
        Vector3 snapTarget = _snapPos + _shakeOffset;
        float pixelSize = (_cam.orthographicSize * 2) / _rtHeight;
        float rightAmount = Vector3.Dot(snapTarget, transform.right);
        float forwardAmount = Vector3.Dot(snapTarget, transform.forward);
        float upAmount = Vector3.Dot(snapTarget, transform.up);
        rightAmount = Mathf.Floor(rightAmount / pixelSize) * pixelSize;
        upAmount = Mathf.Floor(upAmount / pixelSize) * pixelSize;
        // 이게 0.05쯤 되는 격자로 나눠서 몇개 쯤 들어가나 보고, 픽셀을 자시 좌표 숫자로 변환
        Vector3 result = transform.right * rightAmount + transform.up * upAmount + transform.forward * forwardAmount;
        transform.position = result;
    }
}
