namespace BetterBetterTeleporter.Adapters;

public interface IItemInfo
{
    string Name { get; }
    string TypeName { get; }
    string DisplayName { get; }
    bool? IsScrap { get; }
    int? Value { get; }
    bool? IsMetal { get; }
    bool? IsWeapon { get; }
    bool? IsPocketed { get; }
    bool? HasBattery { get; }
    float? BatteryCharge { get; }
    bool? IsTwoHanded { get; }
    float? Weight { get; }
}

public sealed class ItemInfo(GrabbableObject source) : IItemInfo
{
    public string Name => TryGet(() => source.itemProperties.name, "itemProperties.name");
    public string TypeName => source.GetType().Name;
    public string DisplayName => TryGet(() => source.itemProperties.itemName, "itemProperties.itemName");
    public bool? IsScrap => TryGet(() => source.itemProperties.isScrap, "itemProperties.isScrap");
    public int? Value => TryGet(() => source.scrapValue, "scrapValue");
    public bool? IsMetal => TryGet(() => source.itemProperties.isConductiveMetal, "itemProperties.isConductiveMetal");
    public bool? IsWeapon => TryGet(() => source.itemProperties.isDefensiveWeapon, "itemProperties.isDefensiveWeapon");
    public bool? IsPocketed => TryGet(() => source.isPocketed, "isPocketed");
    public bool? HasBattery => TryGet(() => source.itemProperties.requiresBattery, "itemProperties.requiresBattery");
    public float? BatteryCharge => TryGet(() => source.insertedBattery?.charge ?? 0, "insertedBattery.charge");
    public bool? IsTwoHanded => TryGet(() => source.itemProperties.twoHanded, "itemProperties.twoHanded");
    public float? Weight => TryGet(() => source.itemProperties.weight, "itemProperties.weight");

    private static T TryGet<T>(System.Func<T> getter, string propertyName)
    {
        try
        {
            return getter();
        }
        catch (System.Exception e)
        {
            Plugin.Logger.LogError($"Failed to read GrabbableObject.'{propertyName}'. Game structure may have changed. Error: {e.Message}");
            return default!;
        }
    }
}