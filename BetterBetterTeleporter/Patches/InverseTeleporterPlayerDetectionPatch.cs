using HarmonyLib;
using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;

namespace BetterBetterTeleporter.Patches;

/// <summary>
/// This is a patch to keep track of players that are currently being teleported out using the inverse teleporter.
/// Used by KeepItemsOnTeleportPatch.cs
/// </summary>
[HarmonyPatch(typeof(ShipTeleporter), "TeleportPlayerOutWithInverseTeleporter")]
public static class InverseTeleporterPlayerDetectionPatch
{
    private static readonly HashSet<int> InverseTeleportingPlayers = [];
    public static bool IsInverseTeleporting(PlayerControllerB player) => InverseTeleportingPlayers.Contains((int)player.playerClientId);
    [HarmonyPrefix]
    public static void TeleportPlayerOutWithInverseTeleporterPrefix(int playerObj, Vector3 teleportPos) => InverseTeleportingPlayers.Add(playerObj);
    [HarmonyPostfix]
    public static void TeleportPlayerOutWithInverseTeleporterPostfix(int playerObj, Vector3 teleportPos) => InverseTeleportingPlayers.Remove(playerObj);
}