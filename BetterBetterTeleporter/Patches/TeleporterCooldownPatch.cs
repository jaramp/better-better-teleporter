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
        ModConfig.OnCooldownSettingsChanged += UpdateAllTeleporterCooldowns;
    }

    [HarmonyPatch("Awake"), HarmonyPostfix]
    public static void AwakePostfix(ShipTeleporter __instance)
    {
        var (inverse, regular) = (ModConfig.CurrentSettings.InverseTeleporterCooldown, ModConfig.CurrentSettings.TeleporterCooldown);
        if (__instance.isInverseTeleporter)
        {
            __instance.cooldownAmount = inverse;
            Plugin.Logger.LogDebug($"Inverse Teleporter cooldown set to {inverse}s");
        }
        else
        {
            __instance.cooldownAmount = regular;
            Plugin.Logger.LogDebug($"Teleporter cooldown set to {regular}s");
        }
    }

    public static void UpdateAllTeleporterCooldowns(ModConfigData data)
    {
        var localData = data;
        var (inverse, regular) = (localData.InverseTeleporterCooldown, localData.TeleporterCooldown);
        foreach (ShipTeleporter tp in Object.FindObjectsOfType<ShipTeleporter>())
        {
            tp.cooldownAmount = tp.isInverseTeleporter ? inverse : regular;
            cooldownTimeField?.SetValue(tp, Mathf.Min(tp.cooldownAmount, (float)(cooldownTimeField?.GetValue(tp) ?? 0)));
        }
    }

}