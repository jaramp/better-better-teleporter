namespace BetterBetterTeleporter.Adapters;

public interface IItemInfo
{
    string Name { get; }
    string DisplayName { get; }
    string TypeId { get; }
    bool IsScrap { get; }
    int Value { get; }
    bool IsMetal { get; }
    bool IsWeapon { get; }
    bool IsPocketed { get; }
    bool HasBattery { get; }
    float BatteryCharge { get; }
    bool IsTwoHanded { get; }
    float Weight { get; }
}

public sealed class ItemInfo(GrabbableObject source) : IItemInfo
{
    public string Name => source.itemProperties.name;
    public string DisplayName => source.itemProperties.itemName;
    public string TypeId => source.GetType().Name;
    public bool IsScrap => source.itemProperties.isScrap;
    public int Value => source.scrapValue;
    public bool IsMetal => source.itemProperties.isConductiveMetal;
    public bool IsWeapon => source.itemProperties.isDefensiveWeapon;
    public bool IsPocketed => source.isPocketed;
    public bool HasBattery => source.itemProperties.requiresBattery;
    public float BatteryCharge => source.insertedBattery?.charge ?? 0;
    public bool IsTwoHanded => source.itemProperties.twoHanded;
    public float Weight => source.itemProperties.weight;
}