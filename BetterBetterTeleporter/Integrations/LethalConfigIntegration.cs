using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using Unity.Netcode;
using BetterBetterTeleporter.Utility;

namespace BetterBetterTeleporter.Integrations;

internal static class LethalConfigIntegration
{
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void Initialize()
    {
        Plugin.Logger.LogInfo("LethalConfig detected — integrating BetterBetterTeleporter config.");
        RegisterAll();
    }

    private static void RegisterAll()
    {
        // # General
        RegisterCheckbox(Plugin.ModConfig.ResetCooldownOnOrbit.Entry);

        // # Teleporter
        RegisterInput(Plugin.ModConfig.TeleporterCooldown.Entry);
        RegisterDropdown(Plugin.ModConfig.TeleporterBehavior.Entry);
        RegisterTextArea(Plugin.ModConfig.TeleporterAlwaysKeep.Entry);
        RegisterTextArea(Plugin.ModConfig.TeleporterAlwaysDrop.Entry);

        // # Inverse Teleporter
        RegisterInput(Plugin.ModConfig.InverseTeleporterCooldown.Entry);
        RegisterDropdown(Plugin.ModConfig.InverseTeleporterBehavior.Entry);
        RegisterTextArea(Plugin.ModConfig.InverseTeleporterAlwaysKeep.Entry);
        RegisterTextArea(Plugin.ModConfig.InverseTeleporterAlwaysDrop.Entry);
        RegisterSlider(Plugin.ModConfig.BatteryDrainPercent.Entry, 0, 100);

        // # Help
        RegisterShowInventoryButton();
        RegisterSelfTeleportButton();
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

    private static void RegisterTextArea(ConfigEntry<string> entry, int lines = 3)
    {
        var opts = new TextInputFieldOptions { CharacterLimit = 500, NumberOfLines = 50, RequiresRestart = false };
        LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(entry, opts));
    }

    private static void RegisterShowInventoryButton()
    {
        LethalConfigManager.AddConfigItem(new GenericButtonConfigItem("Help",
        "What items am I holding?",
        "See currently held items to help figure out what to add to the keep/drop lists.",
        "Show inventory", () =>
        {
            var inventory = GameNetworkManager.Instance?.localPlayerController?.ItemSlots;
            if (inventory != null && HUDManager.Instance != null)
            {
                var display = string.Join(",", inventory.Where(x => x != null).Select(x => x.itemProperties.name));
                HUDManager.Instance.DisplayTip("Current Inventory", display, false, false);
            }
        }));
    }

    private static void RegisterSelfTeleportButton()
    {
        LethalConfigManager.AddConfigItem(new GenericButtonConfigItem("Help",
            "Activate Teleporter",
            "Pushes the teleport button. Only works if the Teleporter is unlocked and there are no other players connected to the game.",
            "Beam me up, Scotty",
            () =>
            {
                var hud = HUDManager.Instance;
                var round = StartOfRound.Instance;
                var player = GameNetworkManager.Instance?.localPlayerController;

                if (hud == null || round == null || player == null)
                    return;

                const string failMsg = "No cheating";
                if (NetworkManager.Singleton?.ConnectedClientsList.Count > 1)
                {
                    hud.DisplayTip(failMsg, "There's more than one player in the game.", true, false);
                    return;
                }

                var teleporter = UnityEngine.Object.FindObjectsOfType<ShipTeleporter>().FirstOrDefault(tp => !tp.isInverseTeleporter);
                if (teleporter == null)
                {
                    hud.DisplayTip(failMsg, "No teleporter on the ship.", true, false);
                    return;
                }

                var cooldownTimeField = ReflectionHelper.GetShipTeleporterCooldownTimeField();
                if (cooldownTimeField != null && cooldownTimeField.GetValue(teleporter) is float cooldown && cooldown > 0)
                {
                    hud.DisplayTip(failMsg, "The teleporter is on cooldown.", true, false);
                    return;
                }

                teleporter.PressTeleportButtonOnLocalClient();
            }
        ));
    }
}
