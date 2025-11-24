using BetterBetterTeleporter.Adapters;

namespace BetterBetterTeleporter.Tests.Fakes;

public sealed class FakeClipboardItemInfo : IItemInfo
{
    public string? Name { get; set; } = "clipboard";
    public string? TypeId { get; set; } = nameof(ClipboardItem);
    public string? DisplayName { get; set; } = "Clipboard";
    public bool? IsScrap { get; set; } = false;
    public int? Value { get; set; } = 0;
    public bool? IsMetal { get; set; } = false;
    public bool? IsWeapon { get; set; } = false;
    public bool? IsPocketed { get; set; } = false;
    public bool? HasBattery { get; set; } = false;
    public float? BatteryCharge { get; set; } = 0;
    public bool? IsTwoHanded { get; set; } = false;
    public float? Weight { get; set; } = 0;

    // For test readability
    public bool IsHeld { get => IsPocketed == false; set => IsPocketed = !value; }
}
