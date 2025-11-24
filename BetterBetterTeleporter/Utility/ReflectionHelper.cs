using System;
using System.Reflection;

namespace BetterBetterTeleporter.Utility;

public static class ReflectionHelper
{
    private static FieldInfo _cooldownTimeField;
    private static bool _cooldownTimeFieldChecked;

    public static FieldInfo GetShipTeleporterCooldownTimeField()
    {
        if (_cooldownTimeFieldChecked) return _cooldownTimeField;

        _cooldownTimeFieldChecked = true;
        try
        {
            // ShipTeleporter type is available globally because the assembly is referenced.
            _cooldownTimeField = typeof(ShipTeleporter).GetField("cooldownTime", BindingFlags.Instance | BindingFlags.NonPublic);
            if (_cooldownTimeField == null)
            {
                Plugin.Logger.LogWarning("Reflection failed: Could not find ShipTeleporter.cooldownTime field. Teleporter cooldown features will be disabled.");
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Reflection error finding ShipTeleporter.cooldownTime: {e}");
            _cooldownTimeField = null;
        }

        return _cooldownTimeField;
    }
}