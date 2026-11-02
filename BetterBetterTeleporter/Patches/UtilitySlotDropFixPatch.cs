using GameNetcodeStuff;
using HarmonyLib;

namespace BetterBetterTeleporter.Patches;

[HarmonyPatch(typeof(PlayerControllerB), "DropHeldItem")]
public static class DropItem_BeltItemFix
{
    [HarmonyPrefix]
    public static void DropHeldItemPrefix(PlayerControllerB __instance, GrabbableObject dropItem, ref bool setInShip, ref bool setInElevator)
    {
        if (!TeleportDetectionPatch.IsTeleporting(__instance)) return;
        if (dropItem != __instance.ItemOnlySlot) return;

        setInShip = __instance.isInHangarShipRoom;
        setInElevator = __instance.isInElevator;
    }
}