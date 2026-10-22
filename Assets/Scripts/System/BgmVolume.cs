using UnityEngine;

// BGM AudioSource에 붙이는 컴포넌트: 원래 볼륨에 전체·배경음 설정을 곱해서 적용
[RequireComponent(typeof(AudioSource))]
public class BgmVolume : MonoBehaviour
{
    private AudioSource _source;
    private float _baseVolume;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _baseVolume = _source.volume;
        // 효과음 볼륨(AudioListener.volume)의 영향을 받지 않고 BGM 설정만 따르게 함
        _source.ignoreListenerVolume = true;
    }

    private void OnEnable()
    {
        GameSettings.Changed += Apply;
        Apply();
    }

    private void OnDisable() => GameSettings.Changed -= Apply;

    private void Apply() => _source.volume = _baseVolume * GameSettings.BgmFactor;
}
