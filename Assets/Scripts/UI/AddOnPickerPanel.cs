using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AddOnPickerPanel : PickerPanel<AddOnData>
{
    private AddOnCategory activeCategory;
    private AddOnData[] allAddOns;

    protected override void OnAwake()
    {
        allAddOns = Resources.LoadAll<AddOnData>("Prefabs/AddOnData");
    }

    public void Open(AddOnCategory category, IEnumerable<AddOnData> exclude = null)
    {
        activeCategory = category;
        base.Open(exclude);
    }

    public bool HasEligibleItems(AddOnCategory category, IEnumerable<AddOnData> exclude = null)
    {
        activeCategory = category;
        return base.HasEligibleItems(exclude);
    }

    protected override IEnumerable<AddOnData> GetAllItems() =>
        allAddOns.Where(a => a.validCategories.HasFlag(activeCategory));

    protected override string GetLabel(AddOnData item) => item.addOnName;
}
