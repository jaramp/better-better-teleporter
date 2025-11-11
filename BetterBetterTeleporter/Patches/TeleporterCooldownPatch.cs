using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace BetterBetterTeleporter.Patches;

[HarmonyPatch(typeof(ShipTeleporter))]
public static class TeleporterCooldownPatch
{
    private static readonly FieldInfo cooldownTimeField = typeof(ShipTeleporter).GetField("cooldownTime", BindingFlags.Instance | BindingFlags.NonPublic);

    static TeleporterCooldownPatch()
    {
        ConfigSettings.OnCooldownSettingsChanged += UpdateAllTeleporterCooldowns;
    }

    [HarmonyPatch("Awake"), HarmonyPostfix]
    public static void AwakePostfix(ShipTeleporter __instance, ref float ___cooldownAmount, bool ___isInverseTeleporter)
    {
        if (___isInverseTeleporter)
        {
            ___cooldownAmount = ConfigSettings.CurrentSettings.InverseTeleporterCooldown;
            Plugin.Logger.LogDebug($"Inverse Teleporter cooldown set to {___cooldownAmount}s");
        }
        else
        {
            ___cooldownAmount = ConfigSettings.CurrentSettings.TeleporterCooldown;
            Plugin.Logger.LogDebug($"Teleporter cooldown set to {___cooldownAmount}s");
        }
    }

    public static void UpdateAllTeleporterCooldowns()
    {
        Plugin.Logger.LogInfo("Updating all teleporter cooldowns.");
        foreach (ShipTeleporter teleporter in Object.FindObjectsOfType<ShipTeleporter>())
        {
            if (teleporter.isInverseTeleporter)
            {
                teleporter.cooldownAmount = ConfigSettings.CurrentSettings.InverseTeleporterCooldown;
                Plugin.Logger.LogDebug($"Inverse Teleporter cooldown set to {teleporter.cooldownAmount}s");
            }
            else
            {
                teleporter.cooldownAmount = ConfigSettings.CurrentSettings.TeleporterCooldown;
                Plugin.Logger.LogDebug($"Teleporter cooldown set to {teleporter.cooldownAmount}s");
            }

            // Reduce current cooldown timer if it's greater than new maximum
            if (cooldownTimeField != null)
            {
                var currentCooldownTime = (float)(cooldownTimeField.GetValue(teleporter) ?? 0);
                cooldownTimeField.SetValue(teleporter, Mathf.Min(currentCooldownTime, teleporter.cooldownAmount));
            }
        }
    }
}