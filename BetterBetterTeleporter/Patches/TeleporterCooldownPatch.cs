using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace BetterBetterTeleporter.Patches;

/// <summary>
/// This patch class is responsible for managing the cooldown time of the teleporters.
/// It allows configuring different cooldown times for the regular teleporter and the inverse teleporter.
/// </summary>
[HarmonyPatch(typeof(ShipTeleporter))]
public static class TeleporterCooldownPatch
{
    private static readonly FieldInfo cooldownTimeField = typeof(ShipTeleporter).GetField("cooldownTime", BindingFlags.Instance | BindingFlags.NonPublic);

    static TeleporterCooldownPatch()
    {
        ConfigSettings.OnCooldownSettingsChanged += UpdateAllTeleporterCooldowns;
    }

    /// <summary>
    /// This Harmony Postfix patch runs after the Awake method of the ShipTeleporter class.
    /// It sets the cooldownAmount field based on whether the teleporter is a regular or inverse teleporter.
    /// </summary>
    /// <param name="__instance">The ShipTeleporter instance.</param>
    /// <param name="___cooldownAmount">The cooldown amount field (passed by reference).</param>
    /// <param name="___isInverseTeleporter">Whether the teleporter is an inverse teleporter.</param>
    [HarmonyPatch("Awake"), HarmonyPostfix]
    public static void AwakePostfix(ShipTeleporter __instance, ref float ___cooldownAmount, bool ___isInverseTeleporter)
    {
        ___cooldownAmount = ___isInverseTeleporter ? ConfigSettings.CurrentSettings.InverseTeleporterCooldown : ConfigSettings.CurrentSettings.TeleporterCooldown;
        Plugin.Logger.LogDebug($"{(___isInverseTeleporter ? "Inverse Teleporter" : "Teleporter")} cooldown set to {___cooldownAmount}s");
    }

    /// <summary>
    /// Updates the cooldown time of all teleporters in the scene.
    /// </summary>
    public static void UpdateAllTeleporterCooldowns()
    {
        Plugin.Logger.LogInfo("Updating all teleporter cooldowns.");
        foreach (ShipTeleporter teleporter in UnityEngine.Object.FindObjectsOfType<ShipTeleporter>())
        {
            bool isInverse = teleporter.isInverseTeleporter;
            teleporter.cooldownAmount = isInverse ? ConfigSettings.CurrentSettings.InverseTeleporterCooldown : ConfigSettings.CurrentSettings.TeleporterCooldown;
            Plugin.Logger.LogDebug($"{(isInverse ? "Inverse Teleporter" : "Teleporter")} cooldown set to {teleporter.cooldownAmount}s");

            // Reduce current cooldown timer if it's greater than new maximum
            if (cooldownTimeField != null)
            {
                var currentCooldownTime = (float)(cooldownTimeField.GetValue(teleporter) ?? 0);
                cooldownTimeField.SetValue(teleporter, Mathf.Min(currentCooldownTime, teleporter.cooldownAmount));
            }
        }
    }
}