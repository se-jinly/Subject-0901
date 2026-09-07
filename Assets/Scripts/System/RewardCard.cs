using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardCard : MonoBehaviour
    // 오브젝트에 붙어서 작동을 해야하면 모노비헤이비어를 사용함.
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;
    public Button Button => _button;

    public void Set(Reward reward)
    {
        _titleText.text = reward.Title;
        _descriptionText.text = reward.Description;
    }
}
