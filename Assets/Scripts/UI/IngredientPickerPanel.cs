using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IngredientPickerPanel : PickerPanel<IngredientData>
{
    private IngredientCategory activeCategory;
    private IngredientData[] allIngredients;

    // Loaded lazily: the panel GameObject may start inactive (so Awake/OnAwake won't have
    // run before the first Open()), and Open() builds the list before activating the object.
    private IngredientData[] AllIngredients =>
        allIngredients ??= Resources.LoadAll<IngredientData>("Prefabs/IngredientData");

    public void Open(IngredientCategory category, IEnumerable<IngredientData> exclude = null)
    {
        activeCategory = category;
        base.Open(exclude);
    }

    public bool HasEligibleItems(IngredientCategory category, IEnumerable<IngredientData> exclude = null)
    {
        activeCategory = category;
        return base.HasEligibleItems(exclude);
    }

    protected override IEnumerable<IngredientData> GetAllItems() =>
        AllIngredients.Where(a => a.validCategories.HasFlag(activeCategory));

    protected override string GetLabel(IngredientData item) => item.ingredientName;
}
