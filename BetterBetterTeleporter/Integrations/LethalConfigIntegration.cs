using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;

namespace BetterBetterTeleporter.Integrations;

internal static class LethalConfigIntegration
{
    public static bool Enabled { get; private set; }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void Initialize()
    {
        Enabled = true;
        Plugin.Logger.LogInfo("LethalConfig detected — integrating BetterBetterTeleporter config.");

        LethalConfigManager.SetModDescription("Customize teleporter cooldowns and item behavior.");
        RegisterAll();
    }

    private static void RegisterAll()
    {
        // # General
        RegisterCheckbox(ModConfig.ResetCooldownOnOrbitEntry);

        // # Teleporter
        RegisterInput(ModConfig.TeleporterCooldownEntry);
        RegisterDropdown(ModConfig.TeleporterBehaviorEntry);
        RegisterTextInput(ModConfig.TeleporterAlwaysKeepEntry);
        RegisterTextInput(ModConfig.TeleporterAlwaysDropEntry);

        // # Inverse Teleporter
        RegisterInput(ModConfig.InverseTeleporterCooldownEntry);
        RegisterDropdown(ModConfig.InverseTeleporterBehaviorEntry);
        RegisterTextInput(ModConfig.InverseTeleporterAlwaysKeepEntry);
        RegisterTextInput(ModConfig.InverseTeleporterAlwaysDropEntry);
        RegisterSlider(ModConfig.BatteryDrainPercentEntry, 0f, 1f);
    }

    private static void RegisterInput(ConfigEntry<int> entry)
    {
        var opts = new IntInputFieldOptions { RequiresRestart = false };
        LethalConfigManager.AddConfigItem(new IntInputFieldConfigItem(entry, opts));
    }

    private static void RegisterSlider(ConfigEntry<int> entry, int min, int max)
    {
        var opts = new IntSliderOptions { Min = min, Max = max, RequiresRestart = false };
        LethalConfigManager.AddConfigItem(new IntSliderConfigItem(entry, opts));
    }

    private static void RegisterSlider(ConfigEntry<float> entry, float min, float max)
    {
        var opts = new FloatSliderOptions { Min = min, Max = max, RequiresRestart = false };
        LethalConfigManager.AddConfigItem(new FloatSliderConfigItem(entry, opts));
    }

    private static void RegisterCheckbox(ConfigEntry<bool> entry)
    {
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(entry, requiresRestart: false));
    }

    private static void RegisterDropdown(ConfigEntry<ItemTeleportBehavior> entry)
    {
        var opts = new EnumDropDownOptions { RequiresRestart = false };
        LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<ItemTeleportBehavior>(entry, opts));
    }

    private static void RegisterTextInput(ConfigEntry<string> entry)
    {
        LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(entry, requiresRestart: false));
    }
}
