using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BetterBetterTeleporter.Adapters;
using BetterBetterTeleporter.Utility;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
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
        if (!TeleportDetectionPatch.IsTeleporting(__instance)) return;

        var restoreOnCatch = (GrabbableObject[])__instance.ItemSlots.Clone();
        try
        {
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
        catch (System.Exception e)
        {
            Plugin.Logger.LogError($"Failed to intercept DropAllHeldItems (Prefix). Falling back to native behavior. Error: {e}");
            // Return true (default behavior) so the original DropAllHeldItems runs normally.
            tempInventories.Remove(__instance);
            for (int i = 0; i < __instance.ItemSlots.Length; i++)
            {
                __instance.ItemSlots[i] = restoreOnCatch[i];
            }
        }
    }

    [HarmonyPostfix]
    public static void DropAllHeldItemsPostfix(PlayerControllerB __instance)
    {
        if (!tempInventories.ContainsKey(__instance)) return;

        // Restore player's inventory from temporary storage
        var itemsToKeep = tempInventories[__instance];
        tempInventories.Remove(__instance);

        try
        {
            var isInverse = TeleportDetectionPatch.IsInverseTeleporting(__instance);

            float carryWeightDelta = 0f;
            for (int i = 0; i < __instance.ItemSlots.Length; i++)
            {
                var keptItem = itemsToKeep[i];
                if (keptItem == null) continue;

                __instance.ItemSlots[i] = keptItem;
                carryWeightDelta += keptItem.itemProperties.weight - 1f;
            }
            NetworkManager.Singleton.StartCoroutine(RefreshInventory(__instance, carryWeightDelta, isInverse));
        }
        catch (System.Exception e)
        {
            Plugin.Logger.LogError($"Failed to restore inventory. Error: {e}");
        }

        try
        {
            // Force reselect current item slot to fix issues with the player appearing to not have an item equipped
            __instance.isHoldingObject = __instance.ItemSlots[__instance.currentItemSlot] != null;
            SwitchToItemSlotMethod.Invoke(__instance, [__instance.currentItemSlot, null]);
        }
        catch (System.Exception e)
        {
            Plugin.Logger.LogWarning($"Unable to verify current item is being held correctly. Error: {e}");
        }
    }

    private static IEnumerator RefreshInventory(PlayerControllerB __instance, float carryWeightDelta, bool isInverse)
    {
        // Wait for other mods to resolve weight
        yield return new WaitForEndOfFrame();

        // DropAllHeldItems resets weight: need to manually add back current inventory weight
        __instance.carryWeight = Mathf.Clamp(__instance.carryWeight + carryWeightDelta, 1f, 10f);

        // Update inventory items to match new player position
        for (int i = 0; i < __instance.ItemSlots.Length; i++)
        {
            var keptItem = __instance.ItemSlots[i];
            if (keptItem == null) continue;

            keptItem.isInElevator = !isInverse;
            keptItem.isInFactory = isInverse;
            keptItem.isInShipRoom = !isInverse;
        }
    }

    private static TeleporterConfigState GetTeleportState(PlayerControllerB player)
    {
        return new(TeleportDetectionPatch.IsInverseTeleporting(player));
    }
}
