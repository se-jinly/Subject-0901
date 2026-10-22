using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Esc를 누르면 게임을 멈추고 일시정지 메뉴를 띄움. 보상 선택 중, 클리어 후, 사망 후에는 열리지 않음
public class PauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("일시정지 메뉴")]
    [SerializeField] private Player _player;
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _titleButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private SettingsPanel _settingsPanel;
    [Tooltip("다시 시작·타이틀로 이동 기능을 재사용"), SerializeField] private ClearUI _sceneMover;

    private InputAction _pauseAction;

    private void Awake()
    {
        IsPaused = false;
        _panel.SetActive(false);
        _pauseAction = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        _resumeButton.onClick.AddListener(() => SetPaused(false));
        _settingsButton.onClick.AddListener(_settingsPanel.Open);
        _restartButton.onClick.AddListener(() =>
        {
            SetPaused(false);
            _sceneMover.Restart();
        });
        _titleButton.onClick.AddListener(() =>
        {
            SetPaused(false);
            _sceneMover.GoTitle();
        });
        _quitButton.onClick.AddListener(Application.Quit);
    }

    private void OnEnable() => _pauseAction.Enable();
    private void OnDisable() => _pauseAction.Disable();
    private void OnDestroy() => _pauseAction.Dispose();

    private void Update()
    {
        if (!_pauseAction.WasPressedThisFrame()) return;
        if (IsPaused)
        {
            // 설정창이 열려 있으면 메뉴로 돌아가고, 아니면 게임을 재개
            if (_settingsPanel.IsOpen) _settingsPanel.Close();
            else SetPaused(false);
            return;
        }
        // 보상 선택, 클리어 화면 등 이미 입력이 잠긴 상황에서는 열지 않음
        if (_player.InputLocked || _player.IsDead) return;
        SetPaused(true);
    }

    private void SetPaused(bool paused)
    {
        IsPaused = paused;
        _panel.SetActive(paused);
        if (!paused) _settingsPanel.Close();
        _player.InputLocked = paused;
        TimeControl.Set(TimeStopReason.Pause, paused);
    }
}
