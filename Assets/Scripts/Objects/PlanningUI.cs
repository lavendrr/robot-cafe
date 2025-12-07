using UnityEngine;
using TMPro;

public class PlanningUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI dayCountText, moneyText, costText;

    private void Start()
    {
        UpdatePlanningUI();
    }

    public void B_StartShift()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.UIClick);
        StateManager.Instance.ChangeState(new ShiftState());
    }

    private void UpdatePlanningUI()
    {
        dayCountText.text = $"Planning - Day {SaveManager.Instance.GetDayCount()}";
        moneyText.text = $"{SaveManager.Instance.GetPlayerMoney()} Credits";
    }

    public void UpdateFurnitureCostText()
    {
        string colorTag = PlanningManager.Instance.furnitureCost > SaveManager.Instance.GetPlayerMoney() ? "red" : "white";
        costText.text = $"Cost: <color={colorTag}>{PlanningManager.Instance.furnitureCost} Credits</color>";
    }
}
