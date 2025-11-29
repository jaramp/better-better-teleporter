using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace BetterBetterTeleporter.Patches;

[HarmonyPatch]
public class FixTeleporterBugsPatch
{
    private static readonly FieldInfo UpdatePositionForNewlyJoinedClient = AccessTools.Field(typeof(PlayerControllerB), "updatePositionForNewlyJoinedClient");
    private static bool HasFailedToDirtyTeleportPosition = false;

    // Fixes a base game issue where teleporting a player to the same position causes their location to desync until they move
    [HarmonyPatch(typeof(PlayerControllerB), "TeleportPlayer"), HarmonyPrefix]
    public static void TeleportPlayer(PlayerControllerB __instance, ref Vector3 pos)
    {
        if (HasFailedToDirtyTeleportPosition || !TeleportDetectionPatch.IsRegularTeleporting(__instance)) return;
        try
        {
            pos.y -= 0.5f;
            __instance.StartCoroutine(DelayedForceSync(__instance));
        }
        catch
        {
            Plugin.Logger.LogWarning($"Failed to apply teleport positioning fix. Disabling future attempts.");
            HasFailedToDirtyTeleportPosition = true;
        }
    }

    private static System.Collections.IEnumerator DelayedForceSync(PlayerControllerB player)
    {
        yield return new WaitForFixedUpdate();
        UpdatePositionForNewlyJoinedClient?.SetValue(player, true);
        yield return new WaitForSecondsRealtime(0.2f);
        UpdatePositionForNewlyJoinedClient?.SetValue(player, true);
    }
}