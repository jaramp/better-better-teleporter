using GameNetcodeStuff;
using HarmonyLib;

namespace BetterBetterTeleporter.Patches;

[HarmonyPatch]
public class FixTeleporterBugsPatch
{
    // Fixes a base game issue where teleporting a player to the same position causes their location to desync until they move
    private static bool HasFailedToDirtyTeleportPosition = false;
    [HarmonyPatch(typeof(PlayerControllerB), "DropAllHeldItems"), HarmonyPrefix]
    public static void ForceDirtyTeleportPosition(PlayerControllerB __instance)
    {
        if (!TeleportDetectionPatch.IsTeleporting(__instance) || HasFailedToDirtyTeleportPosition) return;
        try
        {
            if (!__instance.IsOwner) return;
            __instance.oldPlayerPosition.Set(__instance.oldPlayerPosition.x - 1, __instance.oldPlayerPosition.y - 1, __instance.oldPlayerPosition.z - 1);
        }
        catch
        {
            Plugin.Logger.LogWarning($"Failed to apply teleport positioning fix. Disabling future attempts.");
            HasFailedToDirtyTeleportPosition = true;
        }
    }
}