using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private RawImage _rawImage;
    [SerializeField] private int _rtWidth = 482; //482 * 272
    [SerializeField] private int _rtHeight = 272; //482 * 272
    [SerializeField] private bool _useSubPixel = true;
    private Vector3 _snapPos; // 부드러운 말고 좀 정확한 픽셀이동 이런식으로 해야겠는데
    private Vector2 _subPixel;
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

        float rightRaw = Vector3.Dot(snapTarget, transform.right);
        float upRaw = Vector3.Dot(snapTarget, transform.up);

        float rightSnapped = Mathf.Floor(rightRaw / pixelSize) * pixelSize;
        float upSnapped = Mathf.Floor(upRaw / pixelSize) * pixelSize;

        float forwardAmount = Vector3.Dot(snapTarget, transform.forward);
        // 이게 0.05쯤 되는 격자로 나눠서 몇개 쯤 들어가나 보고, 픽셀을 자시 좌표 숫자로 변환

        Vector3 result = transform.right * rightSnapped
            + transform.up * upSnapped
            + transform.forward * forwardAmount;
        transform.position = result;

        UpdateSubPixel(rightRaw, rightRaw, upRaw, upSnapped, pixelSize);
        ApplyUVRect();
    }

    // 완성된 화면을 0.3밀어서 보여주는 느낌.
    private void UpdateSubPixel(
        float rightRaw, float rightSnapped, float upRaw, float upSnapped, float pixelSize)
    {
        if (!_useSubPixel)
        {
            _subPixel = Vector2.zero;
            return;
        }
        float restRight = rightRaw - rightSnapped;
        float restUp = upRaw - upSnapped;
        _subPixel.x = restRight / pixelSize;
        _subPixel.y = restUp / pixelSize;
    }
    private void ApplyUVRect()
    {
        float texelX = 1f / _rtWidth;
        float texelY = 1f / _rtHeight;

        float x = 1 + _subPixel.x * texelX;
        float y = 1 + _subPixel.y * texelY;

        float width = _cam.orthographicSize * 2 - 2 * texelX;
        float height = _cam.orthographicSize * 2 - 2 * texelY;
    }

    //public Ray ScreenPointToRay()
    //{
    //    return ray;
    //}
}
