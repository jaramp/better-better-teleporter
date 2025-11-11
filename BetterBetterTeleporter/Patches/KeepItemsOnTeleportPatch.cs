using HarmonyLib;
using GameNetcodeStuff;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

namespace BetterBetterTeleporter.Patches;

/// <summary>
/// This patch class handles the logic for keeping or dropping items when a player teleports.
/// It uses Harmony patches to intercept the DropAllHeldItems method and modify the item dropping behavior.
/// </summary>
[HarmonyPatch(typeof(PlayerControllerB), "DropAllHeldItems")]
public static class KeepItemsOnTeleporterPatch
{
    /// <summary>
    /// Temporarily stores the items that a player should keep when teleporting.
    /// </summary>
    private static readonly Dictionary<PlayerControllerB, GrabbableObject[]> playerItems = [];

    /// <summary>
    /// A reflection-based reference to the SwitchToItemSlot method, used to force a re-selection of the current item slot after teleporting.
    /// </summary>
    private static readonly MethodInfo SwitchToItemSlotMethod = AccessTools.Method(typeof(PlayerControllerB), "SwitchToItemSlot");

    /// <summary>
    /// This Harmony Prefix patch runs before the original DropAllHeldItems method.
    /// It determines which items the player should keep based on the teleport configuration and stores them.
    /// </summary>
    /// <param name="__instance">The PlayerControllerB instance.</param>
    [HarmonyPrefix]
    public static void DropAllHeldItemsPrefix(PlayerControllerB __instance)
    {
        // If the player is not teleporting, exit this patch.
        if (!IsDroppingItemsFromTeleport(__instance)) return;
        Plugin.Logger.LogDebug($"Player {__instance.playerClientId} inventory before teleport: {string.Join(",", __instance.ItemSlots.Select(x => x?.itemProperties.itemName ?? "<empty>"))}");

        var (behavior, oppositeBehaviorItems) = GetTeleportConfig(__instance);
        var itemsToKeep = (GrabbableObject[])__instance.ItemSlots.Clone();
        for (int i = 0; i < __instance.ItemSlots.Length; i++)
        {
            if (ShouldDropItem(__instance.ItemSlots[i], behavior, oppositeBehaviorItems))
            {
                // Remove dropped items from cloned inventory, as these will be dropped on teleport
                itemsToKeep[i] = null;
            }
            else
            {
                // Setting the item slot to null will make DropAllHeldItems ignore it, tricking it into leaving the item in the player's inventory
                __instance.ItemSlots[i] = null;
            }
        }
        Plugin.Logger.LogDebug($"Player {__instance.playerClientId} items to keep: {string.Join(",", itemsToKeep.Where(x => x != null).Select(x => x.itemProperties.itemName))}");

        // This is needed to suppress the drop animation
        __instance.isHoldingObject = false;

        // Temporarily store cloned inventory so we can restore it on Postfix
        playerItems[__instance] = itemsToKeep;
    }

    /// <summary>
    /// This Harmony Postfix patch runs after the original DropAllHeldItems method.
    /// It restores the items that the player should keep after teleporting.
    /// </summary>
    /// <param name="__instance">The PlayerControllerB instance.</param>
    [HarmonyPostfix]
    public static void DropAllHeldItemsPostfix(PlayerControllerB __instance)
    {
        // If the player is not teleporting, exit this patch.
        if (!IsDroppingItemsFromTeleport(__instance)) return;

        // Restore player's inventory from temporary storage
        var itemsToKeep = playerItems[__instance];
        playerItems.Remove(__instance);
        float carryWeight = 0;
        for (int i = 0; i < __instance.ItemSlots.Length; i++)
        {
            if (itemsToKeep[i] == null) continue;
            __instance.ItemSlots[i] = itemsToKeep[i];
            carryWeight += itemsToKeep[i].itemProperties.weight - 1f;
        }
        __instance.carryWeight = Mathf.Clamp(__instance.carryWeight + carryWeight, 1f, 10f);

        // Force reselect current item slot to fix issues with the player appearing to not have an item equipped
        __instance.isHoldingObject = __instance.ItemSlots[__instance.currentItemSlot] != null;
        SwitchToItemSlotMethod.Invoke(__instance, [__instance.currentItemSlot, null]);

        Plugin.Logger.LogDebug($"Player {__instance.playerClientId} inventory after teleport: {string.Join(",", __instance.ItemSlots.Select(x => x?.itemProperties.itemName ?? "<empty>"))}");
    }

    /// <summary>
    /// Gets the teleport configuration based on whether the player is using the regular or inverse teleporter.
    /// </summary>
    /// <param name="player">The PlayerControllerB instance.</param>
    /// <returns>A tuple containing the ItemTeleportBehavior and an array of item names that should be treated oppositely.</returns>
    private static (ItemTeleportBehavior behavior, string[] oppositeBehaviorItems) GetTeleportConfig(PlayerControllerB player)
    {
        var config = ConfigSettings.CurrentSettings;
        ItemTeleportBehavior behavior;
        string[] oppositeBehaviorItems;
        bool isInverseTeleporting = player.shipTeleporterId != 1;

        if (isInverseTeleporting)
        {
            behavior = config.InverseTeleporterBehavior;
            var items = behavior == ItemTeleportBehavior.Keep ? config.InverseTeleporterAlwaysDrop : config.InverseTeleporterAlwaysKeep;
            oppositeBehaviorItems = items.Split(',', StringSplitOptions.RemoveEmptyEntries);
            Plugin.Logger.LogDebug($"Player {player.playerClientId} teleporting via InverseTeleporter");
            Plugin.Logger.LogDebug($"Behavior: {behavior} items");
        }
        else
        {
            behavior = config.TeleporterBehavior;
            var items = behavior == ItemTeleportBehavior.Keep ? config.TeleporterAlwaysDrop : config.TeleporterAlwaysKeep;
            oppositeBehaviorItems = items.Split(',', StringSplitOptions.RemoveEmptyEntries);
            Plugin.Logger.LogDebug($"Player {player.playerClientId} teleporting via Teleporter");
            Plugin.Logger.LogDebug($"Behavior: {behavior} items");
        }

        for (int i = 0; i < oppositeBehaviorItems.Length; i++)
        {
            oppositeBehaviorItems[i] = oppositeBehaviorItems[i].Trim();
        }
        Plugin.Logger.LogDebug($"Except for: {string.Join(",", oppositeBehaviorItems)}");
        return (behavior, oppositeBehaviorItems);
    }

    /// <summary>
    /// Determines whether the player is currently dropping items due to teleporting.
    /// </summary>
    /// <param name="player">The PlayerControllerB instance.</param>
    /// <returns>True if the player is teleporting, false otherwise.</returns>
    private static bool IsDroppingItemsFromTeleport(PlayerControllerB player) => player.shipTeleporterId != -1 || InverseTeleporterPlayerDetectionPatch.IsInverseTeleporting(player);

    /// <summary>
    /// Determines whether a specific item should be dropped based on the teleport behavior and item list.
    /// </summary>
    /// <param name="item">The GrabbableObject to check.</param>
    /// <param name="behavior">The ItemTeleportBehavior.</param>
    /// <param name="itemList">The list of item names to compare against.</param>
    /// <returns>True if the item should be dropped, false otherwise.</returns>
    private static bool ShouldDropItem(GrabbableObject item, ItemTeleportBehavior behavior, string[] itemList)
    {
        if (item == null) return false;

        return behavior == ItemTeleportBehavior.Keep ^ !itemList.Any(x => IsMatchOnName(item, x));
    }

    /// <summary>
    /// Compares multiple item name properties against config value to see if any match.
    /// </summary>
    /// <param name="item">The GrabbableObject to check.</param>
    /// <param name="itemName">The name to attempt to match.</param>
    /// <returns></returns>
    private static bool IsMatchOnName(GrabbableObject item, string itemName)
    {
        const StringComparison caseInsensitive = StringComparison.OrdinalIgnoreCase;
        if (item.itemProperties.itemName.Equals(itemName, caseInsensitive)) return true;
        if (item.itemProperties.name.Equals(itemName, caseInsensitive)) return true;
        if (item.name.Equals(itemName, caseInsensitive)) return true;
        if (item.GetType().ToString().Equals(itemName, caseInsensitive)) return true;
        return false;
    }
}