using UnityEngine;
using TMPro;

public class PlanningUI : MonoBehaviour
{
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
        GameObject.Find("PUI_DayCountText").GetComponent<TextMeshProUGUI>().text = "Planning - Day " + SaveManager.Instance.GetDayCount().ToString();
        GameObject.Find("PUI_MoneyText").GetComponent<TextMeshProUGUI>().text = SaveManager.Instance.GetPlayerMoney().ToString() + " Credits";
    }
}