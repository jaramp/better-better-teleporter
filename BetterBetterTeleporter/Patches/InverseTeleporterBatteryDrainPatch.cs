using System.Linq;
using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace BetterBetterTeleporter.Patches;

[HarmonyPatch(typeof(ShipTeleporter), "TeleportPlayerOutWithInverseTeleporter")]
public static class InverseTeleporterBatteryDrainPatch
{
    private static readonly FieldInfo ItemOnlySlot = AccessTools.Field(typeof(PlayerControllerB), "ItemOnlySlot");

    [HarmonyPostfix]
    public static void TeleportPlayerOutWithInverseTeleporterPostfix(int playerObj)
    {
        float drainAmount = Plugin.ModConfig.BatteryDrainPercent.Value / 100f;
        if (drainAmount == 0) return;

        try
        {
            var player = StartOfRound.Instance.allPlayerScripts[playerObj];
            var items = player.ItemSlots.ToList();
            try { items.Add((GrabbableObject)ItemOnlySlot.GetValue(player)); } catch { }
            foreach (var item in items)
            {
                var battery = item?.insertedBattery;
                battery?.charge = Mathf.Max(0, battery.charge - drainAmount);
            }
        }
        catch (System.Exception e)
        {
            Plugin.Logger.LogError($"Failed to drain batteries after inverse teleport: {e}");
        }
    }
}