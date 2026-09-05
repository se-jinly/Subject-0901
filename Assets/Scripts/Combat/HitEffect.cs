using System.Collections;
using UnityEngine;

[System.Serializable]
public class HitFeedback
{
    [Header("흔들림 및 히트스톱")]
    [SerializeField] private float _shakePower = 0.1f;
    [SerializeField] private float _shakeDuration = 0.1f;
    [SerializeField] private float _hitStopDuration = 0.04f;
    [SerializeField] private int _maxMultiplier = 3;

    public void Play(int hitCount)
        // 여러마리 맞췄을 때 쾌감을 곱하기 위해서
    {
        if (hitCount <= 0) return;
        int power = Mathf.Min(hitCount, _maxMultiplier);
        HitEffect.Instance.Execute(_shakePower * power, _shakeDuration * power, _hitStopDuration * power);
    }
}

public class HitEffect : MonoBehaviour
{
    public static HitEffect Instance { get; private set; }
    // 이게 거의 본체임 모든 타격을 하나의 인스턴스 안에서 관리함.
    private Coroutine _hitStopRoutine;
    private CameraFollowing _camera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            // 이미 돌던게 있으면 나중에 들어온 놈을 파괴
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _camera = Camera.main.GetComponent<CameraFollowing>();
    }

    public void Execute(float shakePower, float shakeDuration, float hitStopDuration)
    {

        _camera.Shake(shakePower, shakeDuration);
        if (_hitStopRoutine != null) StopCoroutine(_hitStopRoutine);
        // 루틴 끊으면 다시 복구시켜야함.
        Time.timeScale = 1f;
        _hitStopRoutine = StartCoroutine(HitStop(hitStopDuration));
    }
    private IEnumerator HitStop(float hitStopDuration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;
    }


}


