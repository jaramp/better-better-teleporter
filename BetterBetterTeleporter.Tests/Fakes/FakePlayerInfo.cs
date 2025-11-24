using BetterBetterTeleporter.Adapters;

namespace BetterBetterTeleporter.Tests.Fakes;

public sealed class FakePlayerInfo : IPlayerInfo
{
    private IItemInfo?[] _slots = new IItemInfo?[4];
    public IReadOnlyList<IItemInfo?> Slots => _slots;
    public int CurrentItemSlotIndex => _slots[0]?.IsPocketed == true ? 1 : 0;

    public FakePlayerInfo(IItemInfo item)
    {
        _slots[0] = item;
    }
}