using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;

namespace BetterBetterTeleporter.Adapters;

public interface IPlayerInfo
{
    IReadOnlyList<IItemInfo> Slots { get; }
    IItemInfo ItemOnlySlot { get; }
    int CurrentItemSlotIndex { get; }
}


public sealed class PlayerInfo(PlayerControllerB player) : IPlayerInfo
{
    private readonly IReadOnlyList<IItemInfo> _slots = [.. TryGet(() => player.ItemSlots.Select(item => item == null ? null : new ItemInfo(item)), "ItemSlots") ?? []];
    public IReadOnlyList<IItemInfo> Slots => _slots;
    private readonly IItemInfo _itemOnlySlot = TryGet(() => new ItemInfo(player.ItemOnlySlot), "ItemOnlySlot", false);
    public IItemInfo ItemOnlySlot => _itemOnlySlot;
    public int CurrentItemSlotIndex => TryGet(() => player.currentItemSlot, "currentItemSlot");

    private static T TryGet<T>(System.Func<T> getter, string propertyName, bool logError = true)
    {
        try
        {
            return getter();
        }
        catch (System.Exception e)
        {
            if (logError)
            {
                Plugin.Logger.LogError($"Failed to read 'PlayerControllerB.{propertyName}'. Game structure may have changed. Error: {e.Message}");
            }
            return default!;
        }
    }
}