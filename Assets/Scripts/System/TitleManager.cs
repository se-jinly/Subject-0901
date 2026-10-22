using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private SettingsPanel _settingsPanel;
    private InputAction _backAction;

    private void Awake()
    {
        _backAction = new InputAction("Back", InputActionType.Button, "<Keyboard>/escape");
    }

    private void OnEnable() => _backAction.Enable();
    private void OnDisable() => _backAction.Disable();
    private void OnDestroy() => _backAction.Dispose();

    private void Update()
    {
        // 설정창이 열려 있으면 Esc로 닫음
        if (_backAction.WasPressedThisFrame() && _settingsPanel.IsOpen) _settingsPanel.Close();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Battle");
    }

    public void OpenSettings() => _settingsPanel.Open();

    public void EndGame()
    {
        Application.Quit();
    }
}
