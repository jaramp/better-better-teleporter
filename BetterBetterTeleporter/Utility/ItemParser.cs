using System;
using System.Linq;
using BetterBetterTeleporter.Adapters;

namespace BetterBetterTeleporter.Utility;

public static class ItemParser
{
    public static bool ShouldDrop(IPlayerControllerB player, IGrabbableObject item, bool behavior, string[] itemList)
    {
        return item != null && behavior ^ !itemList.Any(itemName => IsMatch(player, item, itemName));
    }

    private static bool IsMatch(IPlayerControllerB player, IGrabbableObject item, string itemName)
    {
        if (itemName[0] == '[' && itemName[^1] == ']')
            return IsMatchOnCategory(player, item, itemName);
        return IsMatchOnName(item, itemName);
    }

    private static bool IsMatchOnCategory(IPlayerControllerB player, IGrabbableObject item, string category)
    {
        category = category.ToLowerInvariant();
        var parts = category[1..^1].Split(":not", 2, StringSplitOptions.RemoveEmptyEntries);
        bool behavior = parts[0] switch
        {
            "current" => player.ItemSlots[player.currentItemSlot] == item,
            _ => false,
        };
        string[] itemList = [];
        if (parts.Length > 1 && parts[1][0] == '(' && parts[1][^1] == ')')
        {
            itemList = parts[1][1..^1].Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
        return behavior && !ShouldDrop(player, item, behavior, itemList);
    }

    private static bool IsMatchOnName(IGrabbableObject item, string itemName)
    {
        const StringComparison caseInsensitive = StringComparison.OrdinalIgnoreCase;
        if (item.itemProperties.name.Equals(itemName, caseInsensitive)) return true; // ExtensionLadder
        if (item.GetType().ToString().Equals(itemName, caseInsensitive)) return true; // ExtensionLadderItem
        if (item.itemProperties.itemName.Equals(itemName, caseInsensitive)) return true; // Extension Ladder
        return false;
    }
}