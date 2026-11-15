using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;

namespace BetterBetterTeleporter.Patches;

[HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.TeleportPlayer))]
internal static class TeleportPositionPatch
{
    private static readonly FieldInfo ServerPosition = AccessTools.Field(typeof(PlayerControllerB), "serverPlayerPosition");

    [HarmonyPostfix]
    private static void TeleportPlayerPostfix(PlayerControllerB __instance)
    {
        if (!__instance.isInElevator) return;
        var localPosition = __instance.transform.localPosition;
        ServerPosition?.SetValue(__instance, localPosition);
    }
}