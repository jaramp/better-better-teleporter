using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace BetterBetterTeleporter.Patches;

[HarmonyPatch(typeof(PlayerControllerB), "DropAllHeldItems")]
public static class KeepItemsOnTeleporterPatch
{
    private static readonly Dictionary<PlayerControllerB, GrabbableObject[]> tempInventories = [];
    private static readonly MethodInfo SwitchToItemSlotMethod = AccessTools.Method(typeof(PlayerControllerB), "SwitchToItemSlot");

    /// <summary>
    /// This Harmony Postfix patch runs before the original DropAllHeldItems method.
    /// It checks if the player is teleporting, and hides items that are meant to be kept.
    /// </summary>
    /// <param name="__instance">The PlayerControllerB instance.</param>
    [HarmonyPrefix]
    public static void DropAllHeldItemsPrefix(PlayerControllerB __instance)
    {
        // If the player is disconnecting, exit this patch.
        if (!StartOfRound.Instance.ClientPlayerList.ContainsKey(__instance.actualClientId)) return;
        // If the player is not teleporting, exit this patch.
        if (!IsDroppingItemsFromTeleport(__instance)) return;

        var (behavior, itemList) = GetTeleportConfig(__instance);
        var itemsToKeep = (GrabbableObject[])__instance.ItemSlots.Clone();
        Plugin.Logger.LogDebug($"Client {__instance.playerClientId} inventory before teleport: {Stringify(__instance.ItemSlots)}");
        for (int i = 0; i < __instance.ItemSlots.Length; i++)
        {
            if (ShouldDrop(__instance, __instance.ItemSlots[i], behavior, itemList))
                itemsToKeep[i] = null; // Remove item from cloned inventory
            else
                __instance.ItemSlots[i] = null; // Hide the item from DropAllHeldItems, tricking it into leaving the item in the player's inventory
        }
        Plugin.Logger.LogDebug($"Client {__instance.playerClientId} items to keep: {Stringify(itemsToKeep.Where(x => x != null))}");
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
        // If the player is disconnecting, exit this patch.
        if (!StartOfRound.Instance.ClientPlayerList.ContainsKey(__instance.actualClientId)) return;
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

        Plugin.Logger.LogDebug($"Client {__instance.playerClientId} inventory after teleport: {Stringify(__instance.ItemSlots)}");
    }

    private static (bool behavior, string[] itemList) GetTeleportConfig(PlayerControllerB player)
    {
        bool isInverse = player.shipTeleporterId != 1;
        bool isKeeping;
        string[] except;

        if (isInverse)
        {
            Plugin.Logger.LogDebug($"Client {player.playerClientId} inverse teleporting...");
            isKeeping = Plugin.ModConfig.InverseTeleporterBehavior.Value == ItemTeleportBehavior.Keep;
            var items = isKeeping ? Plugin.ModConfig.InverseTeleporterAlwaysDrop : Plugin.ModConfig.InverseTeleporterAlwaysKeep;
            except = items.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            Plugin.Logger.LogDebug($"Client {player.playerClientId} teleporting...");
            isKeeping = Plugin.ModConfig.TeleporterBehavior.Value == ItemTeleportBehavior.Keep;
            var items = isKeeping ? Plugin.ModConfig.TeleporterAlwaysDrop : Plugin.ModConfig.TeleporterAlwaysKeep;
            except = items.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }

        for (int i = 0; i < except.Length; i++)
        {
            except[i] = except[i].Trim();
        }
        Plugin.Logger.LogDebug($"Client {player.playerClientId} {(isKeeping ? "keeping" : "dropping")} all items{(except.Length > 0 ? $" except for: {string.Join(",", except)}" : "")}");
        return (isKeeping, except);
    }

    private static bool ShouldDrop(PlayerControllerB player, GrabbableObject item, bool behavior, string[] itemList)
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
        // TODO: Solve issue where Behavior is Drop and Keep List is [current:not(Key)] drops equipped Clipboard
        var parts = category.Split(":not", 1)[1..^1];
        bool behavior = $"{category.ToLowerInvariant()}]" switch
        {
            "current" => player.ItemSlots[player.currentItemSlot] == item,
            _ => false,
        };
        string[] itemList = [];
        if (parts.Length > 1 && parts[1][0] == '(' && parts[1][^1] == ')') {
            itemList = parts[1][1..^1].Split(',');
        }
        return !ShouldDrop(player, item, behavior, itemList);
    }

    private static bool IsMatchOnName(GrabbableObject item, string itemName)
    {
        const StringComparison caseInsensitive = StringComparison.OrdinalIgnoreCase;
        if (item.itemProperties.name.Equals(itemName, caseInsensitive)) return true; // ExtensionLadder
        if (item.GetType().ToString().Equals(itemName, caseInsensitive)) return true; // ExtensionLadderItem
        if (item.itemProperties.itemName.Equals(itemName, caseInsensitive)) return true; // Extension Ladder
        return false;
    }

    private static bool IsDroppingItemsFromTeleport(PlayerControllerB player) => player.shipTeleporterId == 1 || InverseTeleporterPlayerDetectionPatch.IsInverseTeleporting(player);
    private static string Stringify(IEnumerable<GrabbableObject> items) => string.Join(",", items.Select(x => x?.itemProperties.itemName ?? "<empty>"));
}