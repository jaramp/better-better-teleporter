using HarmonyLib;
using GameNetcodeStuff;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

namespace BetterBetterTeleporter.Patches;

[HarmonyPatch(typeof(PlayerControllerB), "DropAllHeldItems")]
public static class KeepItemsOnTeleporterPatch
{
    private static readonly Dictionary<PlayerControllerB, GrabbableObject[]> tempInventories = [];
    private static readonly MethodInfo SwitchToItemSlotMethod = AccessTools.Method(typeof(PlayerControllerB), "SwitchToItemSlot");

    [HarmonyPrefix]
    public static void DropAllHeldItemsPrefix(PlayerControllerB __instance)
    {
        // If the player is not teleporting, exit this patch.
        if (!IsDroppingItemsFromTeleport(__instance)) return;
        Plugin.Logger.LogDebug($"Player {__instance.playerClientId} inventory before teleport: {Stringify(__instance.ItemSlots)}");

        var (behavior, oppositeBehaviorItems) = GetTeleportConfig(__instance);
        var itemsToKeep = (GrabbableObject[])__instance.ItemSlots.Clone();
        for (int i = 0; i < __instance.ItemSlots.Length; i++)
        {
            if (ShouldDropItem(__instance.ItemSlots[i], behavior, oppositeBehaviorItems))
            {
                // Remove item from cloned inventory, as it will be dropped on teleport
                itemsToKeep[i] = null;
            }
            else
            {
                // Setting the item slot to null will make DropAllHeldItems ignore it, tricking it into leaving the item in the player's inventory
                __instance.ItemSlots[i] = null;
            }
        }
        Plugin.Logger.LogDebug($"Player {__instance.playerClientId} items to keep: {Stringify(itemsToKeep.Where(x => x != null))}");

        // This is needed to suppress the drop animation
        __instance.isHoldingObject = false;

        // Temporarily store cloned inventory so we can restore it on Postfix
        tempInventories[__instance] = itemsToKeep;
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
        var itemsToKeep = tempInventories[__instance];
        tempInventories.Remove(__instance);
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

        Plugin.Logger.LogDebug($"Player {__instance.playerClientId} inventory after teleport: {Stringify(__instance.ItemSlots)}");
    }

    private static (ItemTeleportBehavior behavior, string[] oppositeBehaviorItems) GetTeleportConfig(PlayerControllerB player)
    {
        var config = ModConfig.CurrentSettings;
        ItemTeleportBehavior behavior;
        string[] oppositeBehaviorItems;
        bool isInverseTeleporting = player.shipTeleporterId != 1;

        if (isInverseTeleporting)
        {
            Plugin.Logger.LogDebug($"Player {player.playerClientId} teleporting via InverseTeleporter");
            behavior = config.InverseTeleporterBehavior;
            var items = behavior == ItemTeleportBehavior.Keep ? config.InverseTeleporterAlwaysDrop : config.InverseTeleporterAlwaysKeep;
            oppositeBehaviorItems = items.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            Plugin.Logger.LogDebug($"Player {player.playerClientId} teleporting via Teleporter");
            behavior = config.TeleporterBehavior;
            var items = behavior == ItemTeleportBehavior.Keep ? config.TeleporterAlwaysDrop : config.TeleporterAlwaysKeep;
            oppositeBehaviorItems = items.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
        Plugin.Logger.LogDebug($"Behavior: {behavior} items");

        for (int i = 0; i < oppositeBehaviorItems.Length; i++)
        {
            oppositeBehaviorItems[i] = oppositeBehaviorItems[i].Trim();
        }
        Plugin.Logger.LogDebug($"Except for: {string.Join(",", oppositeBehaviorItems)}");
        return (behavior, oppositeBehaviorItems);
    }

    private static bool ShouldDropItem(GrabbableObject item, ItemTeleportBehavior behavior, string[] itemList)
    {
        if (item == null) return false;

        return behavior == ItemTeleportBehavior.Keep ^ !itemList.Any(x => IsMatchOnName(item, x));
    }

    private static bool IsMatchOnName(GrabbableObject item, string itemName)
    {
        const StringComparison caseInsensitive = StringComparison.OrdinalIgnoreCase;
        if (item.itemProperties.itemName.Equals(itemName, caseInsensitive)) return true;
        if (item.itemProperties.name.Equals(itemName, caseInsensitive)) return true;
        if (item.name.Equals(itemName, caseInsensitive)) return true;
        if (item.GetType().ToString().Equals(itemName, caseInsensitive)) return true;
        return false;
    }

    private static bool IsDroppingItemsFromTeleport(PlayerControllerB player) => player.shipTeleporterId != -1 || InverseTeleporterPlayerDetectionPatch.IsInverseTeleporting(player);
    private static string Stringify(IEnumerable<GrabbableObject> items) => string.Join(",", items.Select(x => x?.itemProperties.itemName ?? "<empty>"));
}