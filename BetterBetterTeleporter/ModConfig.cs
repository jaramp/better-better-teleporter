using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;

namespace BetterBetterTeleporter;

public enum ItemTeleportBehavior { Keep, Drop }

public class ModConfig
{
    public ConfigEntry<bool> ResetCooldownOnOrbit;
    public ConfigEntry<int> TeleporterCooldown;
    public ConfigEntry<ItemTeleportBehavior> TeleporterBehavior;
    public ConfigEntry<string> TeleporterAlwaysKeep;
    public ConfigEntry<string> TeleporterAlwaysDrop;
    public ConfigEntry<int> InverseTeleporterCooldown;
    public ConfigEntry<ItemTeleportBehavior> InverseTeleporterBehavior;
    public ConfigEntry<string> InverseTeleporterAlwaysKeep;
    public ConfigEntry<string> InverseTeleporterAlwaysDrop;
    public ConfigEntry<int> BatteryDrainPercent;

    public ModConfig(ConfigFile config)
    {
        config.SaveOnConfigSet = false;

        // # General
        ResetCooldownOnOrbit = config.Bind("General", "ResetCooldownOnOrbit", false, new ConfigDescription("Resets the cooldown time on teleporters between days."));

        // # Teleporter
        TeleporterCooldown = config.Bind("Teleporter", "TeleporterCooldown", 10, new ConfigDescription("Cooldown time (in seconds) for using the Teleporter.", new AcceptableValueRange<int>(0, int.MaxValue)));
        TeleporterBehavior = config.Bind("Teleporter", "TeleporterBehavior", ItemTeleportBehavior.Drop, new ConfigDescription("Makes the Teleporter \"Drop\" or \"Keep\" items on teleport."));
        TeleporterAlwaysKeep = config.Bind("Teleporter", "TeleporterAlwaysKeep", "", new ConfigDescription("Keep these items regardless of Teleporter behavior (comma-separated item names).\n\nDoes nothing if TeleporterBehavior is set to \"Keep\"."));
        TeleporterAlwaysDrop = config.Bind("Teleporter", "TeleporterAlwaysDrop", "", new ConfigDescription("Drop these items regardless of Teleporter behavior (comma-separated item names).\n\nDoes nothing if TeleporterBehavior is set to \"Drop\"."));

        // # Inverse Teleporter
        InverseTeleporterCooldown = config.Bind("Inverse Teleporter", "InverseTeleporterCooldown", 210, new ConfigDescription("Cooldown time (in seconds) for using the Inverse Teleporter.", new AcceptableValueRange<int>(0, int.MaxValue)));
        InverseTeleporterBehavior = config.Bind("Inverse Teleporter", "InverseTeleporterBehavior", ItemTeleportBehavior.Drop, new ConfigDescription("Makes the Inverse Teleporter \"Drop\" or \"Keep\" items on teleport."));
        InverseTeleporterAlwaysKeep = config.Bind("Inverse Teleporter", "InverseTeleporterAlwaysKeep", "", new ConfigDescription("Keep these items regardless of Inverse Teleporter behavior (comma-separated item names).\n\nDoes nothing if InverseTeleporterBehavior is set to \"Keep\"."));
        InverseTeleporterAlwaysDrop = config.Bind("Inverse Teleporter", "InverseTeleporterAlwaysDrop", "", new ConfigDescription("Drop these items regardless of Inverse Teleporter behavior (comma-separated item names).\n\nDoes nothing if InverseTeleporterBehavior is set to \"Drop\"."));
        BatteryDrainPercent = config.Bind("Inverse Teleporter", "BatteryDrainPercent", 0, new ConfigDescription("Drains all held battery items by a percentage when using the Inverse Teleporter.", new AcceptableValueRange<int>(0, 100)));

        ((Dictionary<ConfigDefinition, string>)AccessTools.Property(typeof(ConfigFile), "OrphanedEntries").GetValue(config)).Clear();
        config.Save();
        config.SaveOnConfigSet = true;
    }
}