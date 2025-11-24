using HarmonyLib;
using UnityEngine;
using BetterBetterTeleporter.Utility;
using System.Reflection;

namespace BetterBetterTeleporter.Patches;

[HarmonyPatch(typeof(ShipTeleporter))]
public static class TeleporterCooldownPatch
{
    static TeleporterCooldownPatch()
    {
        Plugin.ModConfig.TeleporterCooldown.OnChanged += UpdateAllTeleporterCooldowns;
        Plugin.ModConfig.InverseTeleporterCooldown.OnChanged += UpdateAllTeleporterCooldowns;
    }

    [HarmonyPatch("Awake"), HarmonyPostfix]
    public static void AwakePostfix(ShipTeleporter __instance)
    {
        var (inverse, regular) = GetCooldowns();
        __instance.cooldownAmount = __instance.isInverseTeleporter ? inverse : regular;
    }

    private static void UpdateAllTeleporterCooldowns(int oldValue, int newValue)
    {
        if (oldValue == newValue) return;

        var cooldownTimeField = ReflectionHelper.GetShipTeleporterCooldownTimeField();
        if (cooldownTimeField == null) return;

        var (inverse, regular) = GetCooldowns();
        foreach (ShipTeleporter tp in Object.FindObjectsOfType<ShipTeleporter>())
        {
            try
            {
                tp.cooldownAmount = tp.isInverseTeleporter ? inverse : regular;
                cooldownTimeField.SetValue(tp, Mathf.Min(tp.cooldownAmount, (float)(cooldownTimeField.GetValue(tp) ?? 0)));
            }
            catch (System.Exception e)
            {
                Plugin.Logger.LogError($"Error setting cooldown on teleporter {tp.teleporterId}: {e.Message}");
            }
        }
    }

    private static (int inverse, int regular) GetCooldowns() => (Plugin.ModConfig.InverseTeleporterCooldown.Value, Plugin.ModConfig.TeleporterCooldown.Value);
}