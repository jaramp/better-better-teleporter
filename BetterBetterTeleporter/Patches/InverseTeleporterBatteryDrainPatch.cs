using HarmonyLib;
using UnityEngine;

namespace BetterBetterTeleporter.Patches;

[HarmonyPatch(typeof(ShipTeleporter), "TeleportPlayerOutWithInverseTeleporter")]
public static class InverseTeleporterBatteryDrainPatch
{
    [HarmonyPostfix]
    public static void TeleportPlayerOutWithInverseTeleporterPostfix(int playerObj, Vector3 teleportPos)
    {
        var drainAmount = ModConfig.CurrentSettings.BatteryDrainPercent;
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