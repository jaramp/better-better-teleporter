using BetterBetterTeleporter.Adapters;

namespace BetterBetterTeleporter.Tests.Fakes;

public sealed class FakeItemInfo : IItemInfo
{
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string TypeId { get; set; } = "";
    public bool IsScrap { get; set; }
    public int Value { get; set; }
    public bool IsMetal { get; set; }
    public bool IsWeapon { get; set; }
    public bool IsPocketed { get; set; }
    public bool HasBattery { get; set; }
    public float BatteryCharge { get; set; }
    public bool IsTwoHanded { get; set; }
    public float Weight { get; set; }
}
