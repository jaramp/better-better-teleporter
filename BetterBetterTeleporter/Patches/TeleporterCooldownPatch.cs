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
        __instance.cooldownAmount = __instance.isInverseTeleporter ? inverse : regular;
    }

    private static void UpdateAllTeleporterCooldowns(int oldValue, int newValue)
    {
        if (oldValue == newValue) return;

        var (inverse, regular) = GetCooldowns();
        foreach (ShipTeleporter tp in Object.FindObjectsOfType<ShipTeleporter>())
        {
            tp.cooldownAmount = tp.isInverseTeleporter ? inverse : regular;
            cooldownTimeField?.SetValue(tp, Mathf.Min(tp.cooldownAmount, (float)(cooldownTimeField?.GetValue(tp) ?? 0)));
        }
    }

    private static (int inverse, int regular) GetCooldowns() => (Plugin.ModConfig.InverseTeleporterCooldown.Value, Plugin.ModConfig.TeleporterCooldown.Value);
}