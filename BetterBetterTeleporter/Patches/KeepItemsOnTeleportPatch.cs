using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using BetterBetterTeleporter.Adapters;
using BetterBetterTeleporter.Utility;
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

        var playerInfo = new PlayerInfo(__instance);
        var (behavior, itemList) = GetTeleportConfig(__instance);
        var itemsToKeep = (GrabbableObject[])__instance.ItemSlots.Clone();
        Plugin.Logger.LogDebug($"Client {__instance.playerClientId} inventory before teleport: {Stringify(__instance.ItemSlots)}");
        for (int i = 0; i < __instance.ItemSlots.Length; i++)
        {
            if (ItemParser.ShouldDrop(playerInfo, playerInfo.Slots[i], behavior, itemList))
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

    // TODO: Move logic to Utility.ItemParser
    private static (bool behavior, List<ItemRule> rules) GetTeleportConfig(PlayerControllerB player)
    {
        bool isInverse = player.shipTeleporterId != 1;
        bool isKeeping;
        List<ItemRule> rules;

        if (isInverse)
        {
            Plugin.Logger.LogDebug($"Client {player.playerClientId} inverse teleporting...");
            isKeeping = Plugin.ModConfig.InverseTeleporterBehavior.Value == ItemTeleportBehavior.Keep;
            rules = isKeeping ? Plugin.ModConfig.InverseTeleporterDropList : Plugin.ModConfig.InverseTeleporterKeepList;
        }
        else
        {
            Plugin.Logger.LogDebug($"Client {player.playerClientId} teleporting...");
            isKeeping = Plugin.ModConfig.TeleporterBehavior.Value == ItemTeleportBehavior.Keep;
            rules = isKeeping ? Plugin.ModConfig.TeleporterDropList : Plugin.ModConfig.TeleporterKeepList;
        }
        return (isKeeping, rules);
    }

    private static bool IsDroppingItemsFromTeleport(PlayerControllerB player) => player.shipTeleporterId == 1 || InverseTeleporterPlayerDetectionPatch.IsInverseTeleporting(player);
    private static string Stringify(IEnumerable<GrabbableObject> items) => string.Join(",", items.Select(x => x?.itemProperties.itemName ?? "<empty>"));
}