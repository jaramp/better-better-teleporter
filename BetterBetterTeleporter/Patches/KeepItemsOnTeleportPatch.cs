using System.Collections.Generic;
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

    private static bool IsTeleporting(PlayerControllerB player)
    {
        if (!StartOfRound.Instance.ClientPlayerList.ContainsKey(player.actualClientId))
            return false; // Player is disconnecting

        if (player.shipTeleporterId == 1)
            return true; // Regular teleporting

        if (InverseTeleporterPlayerDetectionPatch.IsInverseTeleporting(player))
            return true; // Inverse teleporting

        return false; // Unknown, assume not teleporting
    }

    [HarmonyPrefix]
    public static void DropAllHeldItemsPrefix(PlayerControllerB __instance)
    {
        if (!IsTeleporting(__instance)) return;

        var playerInfo = new PlayerInfo(__instance);
        var state = GetTeleportState(__instance);
        var itemsToKeep = (GrabbableObject[])__instance.ItemSlots.Clone();

        for (int i = 0; i < __instance.ItemSlots.Length; i++)
        {
            if (playerInfo.ShouldDropItem(playerInfo.Slots[i], state)) itemsToKeep[i] = null;
            else __instance.ItemSlots[i] = null; // Hide from DropAllHeldItems to prevent dropping
        }

        // Suppress drop animation call from DropAllHeldItems
        __instance.isHoldingObject = __instance.ItemSlots[__instance.currentItemSlot] != null;

        // Temporarily store cloned inventory so we can restore it on Postfix
        tempInventories[__instance] = itemsToKeep;
    }

    [HarmonyPostfix]
    public static void DropAllHeldItemsPostfix(PlayerControllerB __instance)
    {
        if (!IsTeleporting(__instance)) return;

        // Restore player's inventory from temporary storage
        var itemsToKeep = tempInventories[__instance];
        tempInventories.Remove(__instance);

        // DropAllHeldItems resets weight: need to manually add back current inventory weight
        float carryWeightDelta = 0f;
        for (int i = 0; i < __instance.ItemSlots.Length; i++)
        {
            var keptItem = itemsToKeep[i];
            if (keptItem == null) continue;

            __instance.ItemSlots[i] = keptItem;
            carryWeightDelta += keptItem.itemProperties.weight - 1f;
        }
        __instance.carryWeight = Mathf.Clamp(__instance.carryWeight + carryWeightDelta, 1f, 10f);

        try
        {
            // Force reselect current item slot to fix issues with the player appearing to not have an item equipped
            __instance.isHoldingObject = __instance.ItemSlots[__instance.currentItemSlot] != null;
            SwitchToItemSlotMethod.Invoke(__instance, [__instance.currentItemSlot, null]);
        }
        catch
        {
            Plugin.Logger.LogError("Error running SwitchToItemSlot. This might be caused by an incompatible game version.");
        }
    }

    private static TeleporterConfigState GetTeleportState(PlayerControllerB player)
    {
        return new(InverseTeleporterPlayerDetectionPatch.IsInverseTeleporting(player));
    }
}
