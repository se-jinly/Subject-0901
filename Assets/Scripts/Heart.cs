using UnityEngine;
using UnityEngine.UI;

public class Heart : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private Image _fill;

    public void SetFill(float ratio) => _fill.fillAmount = ratio;

}