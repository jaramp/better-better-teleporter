using System.Collections.Generic;
using BepInEx.Configuration;
using BetterBetterTeleporter.Networking;
using HarmonyLib;

namespace BetterBetterTeleporter;

public enum ItemTeleportBehavior { Keep, Drop }

public class ModConfig
{
    public SyncedEntry<bool> ResetCooldownOnOrbit;
    public SyncedEntry<int> TeleporterCooldown;
    public SyncedEntry<ItemTeleportBehavior> TeleporterBehavior;
    public SyncedEntry<string> TeleporterAlwaysKeep;
    public SyncedEntry<string> TeleporterAlwaysDrop;
    public SyncedEntry<int> InverseTeleporterCooldown;
    public SyncedEntry<ItemTeleportBehavior> InverseTeleporterBehavior;
    public SyncedEntry<string> InverseTeleporterAlwaysKeep;
    public SyncedEntry<string> InverseTeleporterAlwaysDrop;
    public SyncedEntry<int> BatteryDrainPercent;

    public ModConfig(ConfigFile config)
    {
        config.SaveOnConfigSet = false;

        // # General
        ResetCooldownOnOrbit = config.BindSynced("General", "ResetCooldownOnOrbit", false, new ConfigDescription("Resets the cooldown time on teleporters between days."));

        // # Teleporter
        TeleporterCooldown = config.BindSynced("Teleporter", "TeleporterCooldown", 10, new ConfigDescription("Cooldown time (in seconds) for using the Teleporter.", new AcceptableValueRange<int>(0, int.MaxValue)));
        TeleporterBehavior = config.BindSynced("Teleporter", "TeleporterBehavior", ItemTeleportBehavior.Drop, new ConfigDescription("Makes the Teleporter \"Drop\" or \"Keep\" items on teleport."));
        TeleporterAlwaysKeep = config.BindSynced("Teleporter", "TeleporterAlwaysKeep", "", new ConfigDescription("Keep these items regardless of Teleporter behavior (comma-separated item names).\n\nDoes nothing if TeleporterBehavior is set to \"Keep\"."));
        TeleporterAlwaysDrop = config.BindSynced("Teleporter", "TeleporterAlwaysDrop", "", new ConfigDescription("Drop these items regardless of Teleporter behavior (comma-separated item names).\n\nDoes nothing if TeleporterBehavior is set to \"Drop\"."));

        // # Inverse Teleporter
        InverseTeleporterCooldown = config.BindSynced("Inverse Teleporter", "InverseTeleporterCooldown", 210, new ConfigDescription("Cooldown time (in seconds) for using the Inverse Teleporter.", new AcceptableValueRange<int>(0, int.MaxValue)));
        InverseTeleporterBehavior = config.BindSynced("Inverse Teleporter", "InverseTeleporterBehavior", ItemTeleportBehavior.Drop, new ConfigDescription("Makes the Inverse Teleporter \"Drop\" or \"Keep\" items on teleport."));
        InverseTeleporterAlwaysKeep = config.BindSynced("Inverse Teleporter", "InverseTeleporterAlwaysKeep", "", new ConfigDescription("Keep these items regardless of Inverse Teleporter behavior (comma-separated item names).\n\nDoes nothing if InverseTeleporterBehavior is set to \"Keep\"."));
        InverseTeleporterAlwaysDrop = config.BindSynced("Inverse Teleporter", "InverseTeleporterAlwaysDrop", "", new ConfigDescription("Drop these items regardless of Inverse Teleporter behavior (comma-separated item names).\n\nDoes nothing if InverseTeleporterBehavior is set to \"Drop\"."));
        BatteryDrainPercent = config.BindSynced("Inverse Teleporter", "BatteryDrainPercent", 0, new ConfigDescription("Drains all held battery items by a percentage when using the Inverse Teleporter.", new AcceptableValueRange<int>(0, 100)));

        ((Dictionary<ConfigDefinition, string>)AccessTools.Property(typeof(ConfigFile), "OrphanedEntries").GetValue(config)).Clear();
        config.Save();
        config.SaveOnConfigSet = true;
    }
}
