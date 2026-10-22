using System;
using UnityEngine;

// 볼륨·전체화면·화면 흔들림 설정. PlayerPrefs에 저장하고, 게임을 켤 때 자동으로 불러와 적용함
public static class GameSettings
{
    private const string MasterKey = "Settings.Master";
    private const string BgmKey = "Settings.Bgm";
    private const string SfxKey = "Settings.Sfx";
    private const string FullscreenKey = "Settings.Fullscreen";
    private const string ShakeKey = "Settings.Shake";

    private static float _master = 1f;
    private static float _bgm = 1f;
    private static float _sfx = 1f;
    private static bool _shakeEnabled = true;

    // 값이 바뀔 때마다 알림 (BGM 볼륨처럼 직접 계산해야 하는 쪽이 다시 적용하는 용도)
    public static event Action Changed;

    public static float Master
    {
        get => _master;
        set => SetVolume(ref _master, MasterKey, value);
    }

    public static float Bgm
    {
        get => _bgm;
        set => SetVolume(ref _bgm, BgmKey, value);
    }

    public static float Sfx
    {
        get => _sfx;
        set => SetVolume(ref _sfx, SfxKey, value);
    }

    // 창 모드 전환(Alt+Enter 등)도 반영되도록 저장값이 아니라 실제 화면 상태를 읽음
    public static bool Fullscreen
    {
        get => Screen.fullScreen;
        set
        {
            Screen.fullScreenMode = value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
            Changed?.Invoke();
        }
    }

    public static bool ShakeEnabled
    {
        get => _shakeEnabled;
        set
        {
            _shakeEnabled = value;
            PlayerPrefs.SetInt(ShakeKey, value ? 1 : 0);
            Changed?.Invoke();
        }
    }

    // 효과음은 AudioListener.volume으로 조절하고, BGM 소스는 따로 계산해서 적용함 (BgmVolume 참고)
    public static float BgmFactor => _master * _bgm;

    public static void Save() => PlayerPrefs.Save();

    private static void SetVolume(ref float field, string key, float value)
    {
        field = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(key, field);
        ApplySfx();
        Changed?.Invoke();
    }

    private static void ApplySfx() => AudioListener.volume = _master * _sfx;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load()
    {
        _master = PlayerPrefs.GetFloat(MasterKey, 1f);
        _bgm = PlayerPrefs.GetFloat(BgmKey, 1f);
        _sfx = PlayerPrefs.GetFloat(SfxKey, 1f);
        _shakeEnabled = PlayerPrefs.GetInt(ShakeKey, 1) == 1;
        ApplySfx();
        // 저장된 값이 있을 때만 화면 모드를 바꿈 (처음 실행은 빌드 설정을 따름)
        if (PlayerPrefs.HasKey(FullscreenKey))
        {
            Screen.fullScreenMode = PlayerPrefs.GetInt(FullscreenKey) == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        }
    }
}
