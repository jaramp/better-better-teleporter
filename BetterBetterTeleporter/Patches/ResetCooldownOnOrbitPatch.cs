using HarmonyLib;
using UnityEngine;
using BetterBetterTeleporter.Utility;

namespace BetterBetterTeleporter.Patches;

[HarmonyPatch(typeof(StartOfRound))]
public static class ResetCooldownOnOrbitPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("StartGame")]
    [HarmonyPatch("EndOfGame")]
    [HarmonyPatch("EndOfGameClientRpc")]
    private static void ResetCooldowns()
    {
        if (!Plugin.ModConfig.ResetCooldownOnOrbit.Value) return;

        var cooldownTimeField = ReflectionHelper.GetShipTeleporterCooldownTimeField();
        if (cooldownTimeField == null) return;

        foreach (var teleporter in Object.FindObjectsOfType<ShipTeleporter>())
        {
            try
            {
                if ((float)cooldownTimeField.GetValue(teleporter) > 0)
                {
                    cooldownTimeField.SetValue(teleporter, 0);
                    Plugin.Logger.LogDebug($"Reset cooldown on {(teleporter.isInverseTeleporter ? "Inverse Teleporter" : "Teleporter")}.");
                }
            }
            catch (System.Exception e)
            {
                Plugin.Logger.LogError($"Error resetting cooldown on teleporter {teleporter.name}: {e.Message}");
            }
        }
    }
}
