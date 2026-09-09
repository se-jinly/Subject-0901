using UnityEngine;
// 보상 타입을 위한 클래스로 
public enum RewardType
{
    MaxHealth,
    // 최대체력 증가
    Heal,
    // HP 회복
    MoveSpeed,
    // 이속 증가
    AttackDamage,
    // 공격력 증가
    DashCooldown,
    // 대쉬 배수 ex: 0.8배면 쿨타임 80%가 됨, 총 20%감소
}

[System.Serializable] // 애초에 클래스 자체가 저장을 위한 타입임
public class Reward
{
    [SerializeField] private RewardType _type;
    [SerializeField] private string _title;
    [SerializeField, TextArea(2, 3)] private string _description;
    [SerializeField] private float _value;

    public RewardType Type => _type;
    public string Title => _title;
    public string Description => _description;
    public float Value => _value;
}
