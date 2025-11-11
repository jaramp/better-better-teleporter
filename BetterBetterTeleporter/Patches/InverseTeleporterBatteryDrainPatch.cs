using System;
using HarmonyLib;
using UnityEngine;

namespace BetterBetterTeleporter.Patches;

/// <summary>
/// This patch class is responsible for draining the battery of items held by a player when they use the inverse teleporter.
/// </summary>
[HarmonyPatch(typeof(ShipTeleporter), "TeleportPlayerOutWithInverseTeleporter")]
public static class InverseTeleporterBatteryDrainPatch
{
    /// <summary>
    /// This Harmony Postfix patch runs after the TeleportPlayerOutWithInverseTeleporter method.
    /// It drains the battery of each item held by the player based on the configured battery drain percentage.
    /// </summary>
    /// <param name="playerObj">The player object's client ID.</param>
    /// <param name="teleportPos">The teleport position.</param>
    [HarmonyPostfix]
    public static void TeleportPlayerOutWithInverseTeleporterPostfix(int playerObj, Vector3 teleportPos)
    {
        var drainAmount = ConfigSettings.CurrentSettings.BatteryDrainPercent;
        if (drainAmount == 0) return;

        var player = StartOfRound.Instance.allPlayerScripts[playerObj];
        foreach (var item in player.ItemSlots)
        {
            var battery = item?.insertedBattery;
            if (battery != null)
            {
                battery.charge = Mathf.Max(0, battery.charge - drainAmount);
                item.SyncBatteryServerRpc((int)(battery.charge * 100));
            }
        }
    }
}