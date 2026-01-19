using UnityEngine;
using TMPro;
using System.Collections.Generic;

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

        bool validMenu = true;

        List<CafeElement> cafeElements = PlanningManager.Instance.GetFinalGrid();
        List<FurnitureObject> furnitureList = new(); // Might be able to get this in one go with a lambda function?
        Dictionary<MenuItem, FurnitureObject> invalidList = new();
        foreach (CafeElement element in cafeElements)
        {
            furnitureList.Add(element.furnitureObject);
        }

        foreach (MenuItem item in MenuManager.Instance.ListItems())
        {
            foreach (FurnitureObject requiredObject in item.requiredFurniture)
            {
                if (!furnitureList.Contains(requiredObject))
                {
                    validMenu = false;
                    invalidList[item] = requiredObject;
                }
            }
        }

        if (!validMenu)
        {
            foreach (MenuItem item in invalidList.Keys)
            {
                Debug.Log($"{item.name} is missing required furniture object {invalidList[item].furnitureName}");
            }
            return;
        }

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
