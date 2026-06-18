using System.Collections.Generic;

public class FuelPickerPanel : PickerPanel<FuelType>
{
    private readonly FuelType[] allFuels = { FuelType.Unleaded, FuelType.Premium, FuelType.Diesel };

    protected override IEnumerable<FuelType> GetAllItems() => allFuels;
    protected override string GetLabel(FuelType item) => item.ToString();
}
