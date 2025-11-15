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
        // If the player is disconnecting, exit this patch.
        if (!StartOfRound.Instance.ClientPlayerList.ContainsKey(__instance.playerClientId)) return;
        // If the player is not teleporting, exit this patch.
        if (!IsDroppingItemsFromTeleport(__instance)) return;

        var (behavior, itemList) = GetTeleportConfig(__instance);
        var itemsToKeep = (GrabbableObject[])__instance.ItemSlots.Clone();
        for (int i = 0; i < __instance.ItemSlots.Length; i++)
        {
            if (ShouldDropItem(__instance.ItemSlots[i], behavior, itemList))
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
        if (!StartOfRound.Instance.ClientPlayerList.ContainsKey(__instance.playerClientId)) return;
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
    }

    private static (bool behavior, string[] itemList) GetTeleportConfig(PlayerControllerB player)
    {
        var config = ModConfig.CurrentSettings;
        bool isInverse = player.shipTeleporterId != 1;
        bool isKeeping;
        string[] except;

        if (isInverse)
        {
            Plugin.Logger.LogDebug($"Player {player.playerClientId} teleporting via Inverse Teleporter");
            isKeeping = config.IsInverseTeleportKeep;
            var items = isKeeping ? config.InverseTeleporterAlwaysDrop : config.InverseTeleporterAlwaysKeep;
            except = items.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            Plugin.Logger.LogDebug($"Player {player.playerClientId} teleporting via Teleporter");
            isKeeping = config.IsTeleportKeep;
            var items = isKeeping ? config.TeleporterAlwaysDrop : config.TeleporterAlwaysKeep;
            except = items.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
        Plugin.Logger.LogDebug($"Behavior: {isKeeping} items");

        for (int i = 0; i < except.Length; i++)
        {
            except[i] = except[i].Trim();
        }
        Plugin.Logger.LogDebug($"Except for: {string.Join(",", except)}");
        return (isKeeping, except);
    }

    private static bool ShouldDropItem(GrabbableObject item, bool behavior, string[] itemList)
    {
        return item != null && behavior ^ !itemList.Any(x => IsMatchOnName(item, x));
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
}