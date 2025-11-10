using HarmonyLib;
using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;

namespace BetterBetterTeleporter.Patches;

/// <summary>
/// This patch class is responsible for detecting when a player is using the inverse teleporter.
/// It uses a HashSet to keep track of players that are currently being teleported out using the inverse teleporter.
/// </summary>
[HarmonyPatch(typeof(ShipTeleporter), "TeleportPlayerOutWithInverseTeleporter")]
public static class InverseTeleporterPlayerDetectionPatch
{
    /// <summary>
    /// A HashSet to store the player client IDs of players that are currently being teleported out using the inverse teleporter.
    /// </summary>
    private static readonly HashSet<int> InverseTeleportingPlayers = [];

    /// <summary>
    /// Checks if a player is currently being teleported out using the inverse teleporter.
    /// </summary>
    /// <param name="player">The PlayerControllerB instance to check.</param>
    /// <returns>True if the player is being teleported out using the inverse teleporter, false otherwise.</returns>
    public static bool IsInverseTeleporting(PlayerControllerB player) => InverseTeleportingPlayers.Contains((int)player.playerClientId);

    /// <summary>
    /// This Harmony Prefix patch runs before the TeleportPlayerOutWithInverseTeleporter method.
    /// It adds the player's client ID to the InverseTeleportingPlayers HashSet.
    /// </summary>
    /// <param name="playerObj">The player object's client ID.</param>
    /// <param name="teleportPos">The teleport position.</param>
    [HarmonyPrefix]
    public static void TeleportPlayerOutWithInverseTeleporterPrefix(int playerObj, Vector3 teleportPos)
    {
        InverseTeleportingPlayers.Add(playerObj);
    }

    /// <summary>
    /// This Harmony Postfix patch runs after the TeleportPlayerOutWithInverseTeleporter method.
    /// It removes the player's client ID from the InverseTeleportingPlayers HashSet.
    /// </summary>
    /// <param name="playerObj">The player object's client ID.</param>
    /// <param name="teleportPos">The teleport position.</param>
    [HarmonyPostfix]
    public static void TeleportPlayerOutWithInverseTeleporterPostfix(int playerObj, Vector3 teleportPos)
    {
        InverseTeleportingPlayers.Remove(playerObj);
    }
}