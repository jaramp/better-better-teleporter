using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;

namespace BetterBetterTeleporter.Adapters;

public interface IPlayerInfo
{
    IReadOnlyList<IItemInfo> Slots { get; }
    int CurrentSlotIndex { get; }
}


public sealed class PlayerInfo(PlayerControllerB player) : IPlayerInfo
{
    private IReadOnlyList<IItemInfo> _slots = [.. player.ItemSlots.Select(item => item == null ? null : new ItemInfo(item))];
    public IReadOnlyList<IItemInfo> Slots => _slots;
    private readonly int _currentSlotIndex = player.currentItemSlot;
    public int CurrentSlotIndex => _currentSlotIndex;
}