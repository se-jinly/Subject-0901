using UnityEngine;
using UnityEngine.UI;

// 설정창: 볼륨 3개, 전체화면, 화면 흔들림 끄기. Battle과 Title이 같은 프리팹을 씀
public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Toggle _fullscreenToggle;
    [SerializeField] private Toggle _shakeOffToggle;
    [SerializeField] private Button _backButton;

    public bool IsOpen => gameObject.activeSelf;
    public void Open() => gameObject.SetActive(true);
    public void Close() => gameObject.SetActive(false);

    private void Awake()
    {
        _masterSlider.onValueChanged.AddListener(value => GameSettings.Master = value);
        _bgmSlider.onValueChanged.AddListener(value => GameSettings.Bgm = value);
        _sfxSlider.onValueChanged.AddListener(value => GameSettings.Sfx = value);
        _fullscreenToggle.onValueChanged.AddListener(isOn => GameSettings.Fullscreen = isOn);
        _shakeOffToggle.onValueChanged.AddListener(isOn => GameSettings.ShakeEnabled = !isOn);
        _backButton.onClick.AddListener(Close);
    }

    // 열 때마다 현재 값으로 맞춤 (창 모드는 Alt+Enter 등으로 바뀌었을 수 있음)
    private void OnEnable()
    {
        _masterSlider.SetValueWithoutNotify(GameSettings.Master);
        _bgmSlider.SetValueWithoutNotify(GameSettings.Bgm);
        _sfxSlider.SetValueWithoutNotify(GameSettings.Sfx);
        _fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);
        _shakeOffToggle.SetIsOnWithoutNotify(!GameSettings.ShakeEnabled);
    }

    private void OnDisable() => GameSettings.Save();
}
