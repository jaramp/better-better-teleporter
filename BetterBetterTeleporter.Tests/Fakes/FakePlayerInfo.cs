using BetterBetterTeleporter.Adapters;

namespace BetterBetterTeleporter.Tests.Fakes;

public sealed class FakePlayerInfo : IPlayerInfo
{
    public IReadOnlyList<IItemInfo?> Slots { get; init; } = [];
    public int CurrentItemSlotIndex { get; set; }
}