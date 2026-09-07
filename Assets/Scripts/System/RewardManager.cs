using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class RewardManager : MonoBehaviour
{
    [Header("루프")]
    [SerializeField] private Player _player;
    [SerializeField] private GameObject _panel; // 여기에 카드 3장 띄움
    [SerializeField] private RewardCard[] _cards; // 띄울 카드
    [SerializeField] private Reward[] _pool; // 모든 카드 정보
    [SerializeField] private float _openDelay = 0.4f; // 띄우는 딜레이

    private readonly List<Reward> _candidates = new(); // 후보자 보상을 저장해둠
    private bool _isSelecting = false; // 선택하는 중?
    private List<Reward> _rewardDeck = new(); // 나중에 있을 수도 있는 중복방지
    private void Awake()
    {
        _panel.SetActive(false);
        for (int i = 0; i < _cards.Length; i++)
        {
            int index = i;
            _cards[i].Button.onClick.AddListener(() => OnSelect(index));
            // 이건 뭘까?
        }
        _rewardDeck.AddRange(_pool);
    }
    // 보상을 후보자 안에 넣고 그 보상을 카드에 넣어서 UI에 띄움
    public IEnumerator ShowAndWait()
    {
        yield return new WaitForSecondsRealtime(_openDelay);
        PickCandidates(_cards.Length); // 카드의 크기만큼 뽑음.
        for (int i = 0; i < _cards.Length; i++)
        {
            bool hasReward = i < _candidates.Count;
            _cards[i].gameObject.SetActive(hasReward); // 없으면 숨김
            if (hasReward)
            {
                _cards[i].Set(_candidates[i]); // 있으면 넣음
            }
        }
        _panel.SetActive(true);
        _isSelecting = true;
        _player.InputLocked = true;
        Time.timeScale = 0f;
        yield return new WaitUntil(() => _isSelecting == false);
        _panel.SetActive(false);
        _player.InputLocked = false;
        Time.timeScale = 1f;
    }

    private void OnSelect(int index)
    {
        if (!_isSelecting || index < 0 || index >= _candidates.Count) return;
        Apply(_candidates[index]);
        _isSelecting = false;
    }

    private void Apply(Reward reward)
    {
        switch(reward.Type)
        {
            case RewardType.MaxHealth:
                _player.AddMaxHealth(reward.Value);
                break;
            case RewardType.DashCooldown:
                _player.MultiplyDashCooldown(reward.Value);
                break;
            case RewardType.AttackDamage:
                _player.AddAttackDamage(reward.Value);
                break;
            case RewardType.Heal:
                _player.Heal(reward.Value);
                break;
            case RewardType.MoveSpeed:
                _player.AddMoveSpeed(reward.Value);
                break;
            default:
                Debug.LogWarning($"처리 안 된 보상 타입: {reward.Type}");
                break;
        }
        
    }

    private void PickCandidates(int count)
    {
        _candidates.Clear();
        for (int i = 0; i < count; i++)
        {
            int randomCard = Random.Range(0, _rewardDeck.Count);
            _candidates.Add(_rewardDeck[randomCard]);
        }
    }
}
