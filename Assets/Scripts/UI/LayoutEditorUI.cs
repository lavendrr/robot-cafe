using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LayoutEditorUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI dayCountText, moneyText, costText;
    [SerializeField]
    private ErrorableButton saveLayoutButton;
    [SerializeField]
    private SaveChangesModal saveChangesModal;

    // Snapshot of the layout as it was when the editor opened (or was last saved)
    private List<CafeElement> baseline;

    private void Start()
    {
        UpdateLayoutUI();
    }

    private void OnEnable()
    {
        if (PlanningManager.Instance != null)
        {
            PlanningManager.Instance.OnLayoutChanged -= RefreshEditorState;
            PlanningManager.Instance.OnLayoutChanged += RefreshEditorState;
        }

        CaptureBaseline();
        RefreshEditorState();
    }

    private void OnDisable()
    {
        if (PlanningManager.Instance != null)
            PlanningManager.Instance.OnLayoutChanged -= RefreshEditorState;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void RequestClose()
    {
        if (HasUnsavedChanges() && saveChangesModal != null)
            saveChangesModal.Show(onSave: SaveAndClose, onDiscard: DiscardAndClose);
        else
            Close();
    }

    // Modal "Save" path
    private void SaveAndClose()
    {
        if (TrySaveLayout())
            Close();
        else if (saveChangesModal != null)
            saveChangesModal.Hide();
    }

    private void DiscardAndClose()
    {
        if (baseline != null && PlanningManager.Instance != null)
            PlanningManager.Instance.RevertLayout(baseline);
        Close();
    }

    // Save button entry point
    public void SaveLayout()
    {
        TrySaveLayout();
    }

    public bool TrySaveLayout()
    {
        if (PlanningManager.Instance == null || PlanningManager.Instance.gridArray == null)
            return false;

        LevelLayout layout = SaveManager.Instance.GetCafeLayout();
        if (layout == null)
            return false;

        int cost = PlanningManager.Instance.GetUnsavedFurnitureCost();
        if (cost > SaveManager.Instance.GetPlayerMoney())
        {
            if (saveLayoutButton != null)
                saveLayoutButton.FlashError("Not enough credits!", 0.5f);
            return false;
        }

        SaveManager.Instance.AdjustPlayerMoney(-cost);
        layout.elements = PlanningManager.Instance.GetFinalGrid();
        SaveManager.Instance.SaveCafeLayout(layout);

        CaptureBaseline();
        RefreshEditorState();
        return true;
    }

    private void CaptureBaseline()
    {
        baseline = (PlanningManager.Instance != null && PlanningManager.Instance.gridArray != null)
            ? PlanningManager.Instance.GetFinalGrid()
            : new List<CafeElement>();
    }

    public bool HasUnsavedChanges()
    {
        if (baseline == null || PlanningManager.Instance == null || PlanningManager.Instance.gridArray == null)
            return false;

        return !LayoutsEqual(PlanningManager.Instance.GetFinalGrid(), baseline);
    }

    private void RefreshEditorState()
    {
        RefreshSaveButtonState();
        UpdateLayoutUI();
        UpdateFurnitureCostText();
    }

    private void RefreshSaveButtonState()
    {
        if (saveLayoutButton != null)
            saveLayoutButton.SetInteractable(HasUnsavedChanges());
    }

    // Order-independent comparison of two layouts by their placed elements
    private static bool LayoutsEqual(List<CafeElement> a, List<CafeElement> b)
    {
        if (a.Count != b.Count)
            return false;

        List<string> keysA = new(a.Count);
        List<string> keysB = new(b.Count);
        foreach (var e in a) keysA.Add(ElementKey(e));
        foreach (var e in b) keysB.Add(ElementKey(e));
        keysA.Sort();
        keysB.Sort();

        for (int i = 0; i < keysA.Count; i++)
        {
            if (keysA[i] != keysB[i])
                return false;
        }
        return true;
    }

    // Identity of a placed element: which furniture, at which root cell, facing which way
    private static string ElementKey(CafeElement e)
    {
        string name = e.furnitureData != null ? e.furnitureData.name : "null";
        return $"{name}|{e.rootGridCoord.col},{e.rootGridCoord.row}|{e.rotation}";
    }

    private void UpdateLayoutUI()
    {
        dayCountText.text = $"Planning - Day {SaveManager.Instance.GetDayCount()}";
        moneyText.text = $"{SaveManager.Instance.GetPlayerMoney()} Credits";
    }

    public void UpdateFurnitureCostText()
    {
        int cost = PlanningManager.Instance != null ? PlanningManager.Instance.GetUnsavedFurnitureCost() : 0;
        string colorTag = cost > SaveManager.Instance.GetPlayerMoney() ? "red" : "white";
        costText.text = $"Cost: <color={colorTag}>{cost} Credits</color>";
    }
}
