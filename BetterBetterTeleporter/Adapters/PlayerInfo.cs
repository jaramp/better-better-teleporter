using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;

namespace BetterBetterTeleporter.Adapters;

public interface IPlayerInfo
{
    IReadOnlyList<IItemInfo> Slots { get; }
    int CurrentItemSlotIndex { get; }
}


public sealed class PlayerInfo(PlayerControllerB player) : IPlayerInfo
{
    private readonly IReadOnlyList<IItemInfo> _slots = [.. TryGet(() => player.ItemSlots.Select(item => item == null ? null : new ItemInfo(item)), "ItemSlots") ?? []];
    public IReadOnlyList<IItemInfo> Slots => _slots;
    public int CurrentItemSlotIndex => TryGet(() => player.currentItemSlot, "currentItemSlot");

    private static T TryGet<T>(System.Func<T> getter, string propertyName)
    {
        try
        {
            return getter();
        }
        catch (System.Exception e)
        {
            Plugin.Logger.LogError($"Failed to read 'PlayerControllerB.{propertyName}'. Game structure may have changed. Error: {e.Message}");
            return default!;
        }
    }
}