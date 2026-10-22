using TMPro;
using UnityEngine;

// 스킬 칸 위에 남은 쿨타임만큼 반투명한 막을 덮음: 쿨타임이 줄수록 막이 위에서부터 사라짐
public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private SlashSkill _skill;
    [Tooltip("칸 아래쪽에서 시작해 남은 비율만큼 덮는 막"), SerializeField] private RectTransform _cover;
    [Tooltip("남은 시간(초)을 소수점 첫째 자리까지 표시"), SerializeField] private TextMeshProUGUI _timeText;

    private void Update()
    {
        float ratio = _skill.CooldownRatio;
        _cover.gameObject.SetActive(ratio > 0f);
        _cover.anchorMax = new Vector2(1f, ratio);
        _timeText.gameObject.SetActive(ratio > 0f);
        if (ratio > 0f) _timeText.text = _skill.CooldownLeft.ToString("0.0");
    }
}
