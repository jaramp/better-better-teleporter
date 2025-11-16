using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace BetterBetterTeleporter.Patches;

[HarmonyPatch(typeof(ShipTeleporter))]
public static class TeleporterCooldownPatch
{
    private static readonly FieldInfo cooldownTimeField = typeof(ShipTeleporter).GetField("cooldownTime", BindingFlags.Instance | BindingFlags.NonPublic);

    static TeleporterCooldownPatch()
    {
        Plugin.ModConfig.TeleporterCooldown.OnChanged += UpdateAllTeleporterCooldowns;
        Plugin.ModConfig.InverseTeleporterCooldown.OnChanged += UpdateAllTeleporterCooldowns;
    }

    [HarmonyPatch("Awake"), HarmonyPostfix]
    public static void AwakePostfix(ShipTeleporter __instance)
    {
        var (inverse, regular) = GetCooldowns();
        if (__instance.isInverseTeleporter && __instance.cooldownAmount != inverse)
        {
            __instance.cooldownAmount = inverse;
            Plugin.Logger.LogDebug($"Inverse Teleporter cooldown set to {inverse}s");
        }
        else if (__instance.cooldownAmount != regular)
        {
            __instance.cooldownAmount = regular;
            Plugin.Logger.LogDebug($"Teleporter cooldown set to {regular}s");
        }
    }

    private static void UpdateAllTeleporterCooldowns(int oldValue, int newValue)
    {
        Plugin.Logger.LogDebug($"UpdateAllTeleporterCooldowns({oldValue}, {newValue})");
        if (oldValue == newValue) return;

        var (inverse, regular) = GetCooldowns();
        Plugin.Logger.LogDebug($"GetCooldowns = ({oldValue}, {newValue})");
        foreach (ShipTeleporter tp in Object.FindObjectsOfType<ShipTeleporter>())
        {
            tp.cooldownAmount = tp.isInverseTeleporter ? inverse : regular;
            cooldownTimeField?.SetValue(tp, Mathf.Min(tp.cooldownAmount, (float)(cooldownTimeField?.GetValue(tp) ?? 0)));
        }
    }

    private static (int inverse, int regular) GetCooldowns() => (Plugin.ModConfig.InverseTeleporterCooldown.Value, Plugin.ModConfig.TeleporterCooldown.Value);
}