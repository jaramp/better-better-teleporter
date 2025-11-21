using System;
using System.Linq;
using GameNetcodeStuff;

namespace BetterBetterTeleporter.Utility;

public static class ItemParser
{
    public static bool ShouldDrop(PlayerControllerB player, GrabbableObject item, bool behavior, string[] itemList)
    {
        return item != null && behavior ^ !itemList.Any(itemName => IsMatch(player, item, itemName));
    }

    private static bool IsMatch(PlayerControllerB player, GrabbableObject item, string itemName)
    {
        if (itemName[0] == '[' && itemName[^1] == ']')
            return IsMatchOnCategory(player, item, itemName);
        return IsMatchOnName(item, itemName);
    }

    private static bool IsMatchOnCategory(PlayerControllerB player, GrabbableObject item, string category)
    {
        return category switch
        {
            "[current]" => player.ItemSlots[player.currentItemSlot] == item,
            _ => false,
        };

        // category = category.ToLowerInvariant();
        // Plugin.Logger.LogDebug($"Advanced rule: {category}");
        // var parts = category[1..^1].Split(":not", 2, StringSplitOptions.RemoveEmptyEntries);
        // Plugin.Logger.LogDebug($"Category: {parts[0]} {(parts.Length > 1 ? $"| not: {parts[1]}" : "")}");
        // bool behavior = parts[0] switch
        // {
        //     "current" => player.ItemSlots[player.currentItemSlot] == item,
        //     _ => false,
        // };
        // string[] itemList = [];
        // if (parts.Length > 1 && parts[1][0] == '(' && parts[1][^1] == ')')
        // {
        //     itemList = parts[1][1..^1].Split(',', StringSplitOptions.RemoveEmptyEntries);
        //     Plugin.Logger.LogDebug($"itemList: {string.Join(",", itemList)}");
        // }
        // return behavior && !ShouldDrop(player, item, behavior, itemList);
    }

    private static bool IsMatchOnName(GrabbableObject item, string itemName)
    {
        const StringComparison caseInsensitive = StringComparison.OrdinalIgnoreCase;
        if (item.itemProperties.name.Equals(itemName, caseInsensitive)) return true; // ExtensionLadder
        if (item.GetType().ToString().Equals(itemName, caseInsensitive)) return true; // ExtensionLadderItem
        if (item.itemProperties.itemName.Equals(itemName, caseInsensitive)) return true; // Extension Ladder
        return false;
    }
}