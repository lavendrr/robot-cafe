using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LayoutEditorUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI dayCountText, moneyText, costText;

    private void Start()
    {
        UpdateLayoutUI();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void UpdateLayoutUI()
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
