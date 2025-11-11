using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace BetterBetterTeleporter.Patches;

[HarmonyPatch(typeof(StartOfRound))]
public static class ResetCooldownOnOrbitPatch
{
    private static readonly FieldInfo cooldownTimeField = typeof(ShipTeleporter).GetField("cooldownTime", BindingFlags.Instance | BindingFlags.NonPublic);

    [HarmonyPostfix]
    [HarmonyPatch("StartGame")]
    [HarmonyPatch("EndOfGame")]
    [HarmonyPatch("EndOfGameClientRpc")]
    private static void ResetCooldowns()
    {
        if (!ConfigSettings.CurrentSettings.ResetCooldownOnOrbit) return;
        if (cooldownTimeField == null)
        {
            Plugin.Logger.LogWarning("Couldn't find ShipTeleporter.cooldownTime field to reset cooldowns.");
            return;
        }

        foreach (var teleporter in Object.FindObjectsOfType<ShipTeleporter>())
        {
            if ((float)cooldownTimeField.GetValue(teleporter) > 0)
            {
                cooldownTimeField.SetValue(teleporter, 0);
                Plugin.Logger.LogDebug($"Reset cooldown on Teleporter {teleporter.teleporterId}.");
            }
        }
    }
}
