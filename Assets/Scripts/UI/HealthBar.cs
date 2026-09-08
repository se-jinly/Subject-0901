using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private Heart _heartPrefab;
    [SerializeField] private int _heartCapacity = 2;
    private List<Heart> _hearts = new();
    public void SetHealth(int current, int max)
    {
        int needed = Mathf.CeilToInt((float)max / _heartCapacity);
        // hp멕스 예 12 라면 칸당 최대치가 2니까 6개가 필요함
        while(_hearts.Count < needed)
        {
            _hearts.Add(Instantiate(_heartPrefab, transform));
        }
        for (int i = 0; i < _hearts.Count; i++)
        {
            int heartValue = current - i * _heartCapacity;
            // 현제체력에서 용량을 빼면 남은체력이 바로 나옴
            _hearts[i].SetFill(Mathf.Clamp01((float)heartValue / _heartCapacity));
            // 그래서 하트벨류가 5 이런거 들어오면 0에서 1까지 자름
        }
    }
}
// 가끔 2연타 공격하고 스폰 저거 안되게 게임매니져 해야함.
