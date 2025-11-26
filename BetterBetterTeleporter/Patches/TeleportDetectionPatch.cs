using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;

namespace BetterBetterTeleporter.Patches;

/// <summary>
/// This is a patch to keep track of players that are currently being teleported.
/// Used by FixTeleporterBugsPatch.cs and KeepItemsOnTeleportPatch.cs
/// </summary>
[HarmonyPatch(typeof(ShipTeleporter), "TeleportPlayerOutWithInverseTeleporter")]
public static class TeleportDetectionPatch
{
    private static readonly HashSet<int> InverseTeleportingPlayers = [];
    public static bool IsTeleporting(PlayerControllerB player)
    {
        if (!StartOfRound.Instance.ClientPlayerList.ContainsKey(player.actualClientId))
            return false; // Player is disconnecting

        if (IsRegularTeleporting(player)) return true;
        if (IsInverseTeleporting(player)) return true;

        return false; // Unknown, assume not teleporting
    }

    public static bool IsRegularTeleporting(PlayerControllerB player) => player.shipTeleporterId == 1;
    public static bool IsInverseTeleporting(PlayerControllerB player) => InverseTeleportingPlayers.Contains((int)player.playerClientId);
    [HarmonyPrefix]
    public static void TeleportPlayerOutWithInverseTeleporterPrefix(int playerObj) => InverseTeleportingPlayers.Add(playerObj);
    [HarmonyPostfix]
    public static void TeleportPlayerOutWithInverseTeleporterPostfix(int playerObj) => InverseTeleportingPlayers.Remove(playerObj);
}