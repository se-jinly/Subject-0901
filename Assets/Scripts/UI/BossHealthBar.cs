using UnityEngine;

public class BossHealthBar : MonoBehaviour
{
    public static BossHealthBar Instance { get; private set; }

    [SerializeField] private GameObject _root;
    [SerializeField] private RectTransform _fillRect;
    [SerializeField] private float _fullWidth = 1500f;
    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show()
    {
        _root.SetActive(true);
    }
    public void SetHealth(float current, int max)
    {
        float ratio = Mathf.Clamp01(current / max);
        _fillRect.sizeDelta = new Vector2(_fullWidth * ratio, _fillRect.sizeDelta.y);
    }
    public void Hide()
    {
        _root.SetActive(false);
    }
}
