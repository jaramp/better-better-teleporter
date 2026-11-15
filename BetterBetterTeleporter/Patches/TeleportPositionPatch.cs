using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;

namespace BetterBetterTeleporter.Patches;

[HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.TeleportPlayer))]
internal static class TeleportPositionPatch
{
    private static readonly FieldInfo ServerPosition = AccessTools.Field(typeof(PlayerControllerB), "serverPlayerPosition");

    private static void Postfix(PlayerControllerB __instance)
    {
        if (!TeleportDetectionPatch.IsRegularTeleporting()) return;
        ServerPosition?.SetValue(__instance, __instance.transform.localPosition);
    }
}