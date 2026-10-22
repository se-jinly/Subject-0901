using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    public static CoinUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _coinText;
    private int _coin;

    private void Awake()
    {
        Instance = this;
        Refresh();
    }

    public void Add(int amount)
    {
        _coin += amount;
        Refresh();
    }

    // 코인이 충분하면 차감하고 true, 부족하면 아무것도 안 하고 false
    public bool TrySpend(int cost)
    {
        if (_coin < cost) return false;
        _coin -= cost;
        Refresh();
        return true;
    }

    private void Refresh()
    {
        _coinText.text = $"제물: {_coin}";
    }
}
