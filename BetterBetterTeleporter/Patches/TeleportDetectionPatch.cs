using System.Collections;
using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;

namespace BetterBetterTeleporter.Patches;

/// <summary>
/// This is a patch to keep track of players that are currently being teleported.
/// Used by FixTeleporterBugsPatch.cs and KeepItemsOnTeleportPatch.cs
/// </summary>
[HarmonyPatch(typeof(ShipTeleporter))]
public static class TeleportDetectionPatch
{
    public static bool IsTeleporting(PlayerControllerB player)
    {
        if (!StartOfRound.Instance.ClientPlayerList.ContainsKey(player.actualClientId))
            return false; // Player is disconnecting

        if (player.isPlayerDead || player.disableInteract)
            return false; // Player is incapacitated

        if (IsRegularTeleporting()) return true;
        if (IsInverseTeleporting(player)) return true;

        return false; // Unknown, assume not teleporting
    }

    private static bool isTeleporting;

    [HarmonyPatch("beamUpPlayer")]
    [HarmonyPostfix]
    private static void BeamUpPlayerPostfix(ref IEnumerator __result)
    {
        __result = WrappedBeamUpPlayer(__result);
    }

    private static IEnumerator WrappedBeamUpPlayer(IEnumerator original)
    {
        Plugin.Logger.LogDebug("Enabling teleport item logic");
        BeforeTeleporterDropAllHeldItems();

        try
        {
            while (original.MoveNext())
                yield return original.Current;
        }
        finally
        {
            AfterTeleporterDropAllHeldItems();
            Plugin.Logger.LogDebug("Disabling teleport item logic");
        }
    }

    public static void BeforeTeleporterDropAllHeldItems() => isTeleporting = true;
    public static void AfterTeleporterDropAllHeldItems() => isTeleporting = false;
    public static bool IsRegularTeleporting() => isTeleporting;

    private static readonly HashSet<int> InverseTeleportingPlayers = [];

    public static bool IsInverseTeleporting(PlayerControllerB player)
        => InverseTeleportingPlayers.Contains((int)player.playerClientId);

    [HarmonyPatch("TeleportPlayerOutWithInverseTeleporter")]
    [HarmonyPrefix]
    public static void TeleportPlayerOutWithInverseTeleporterPrefix(int playerObj)
        => InverseTeleportingPlayers.Add(playerObj);

    [HarmonyPatch("TeleportPlayerOutWithInverseTeleporter")]
    [HarmonyPostfix]
    public static void TeleportPlayerOutWithInverseTeleporterPostfix(int playerObj)
        => InverseTeleportingPlayers.Remove(playerObj);
}