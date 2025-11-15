using System.Collections.Generic;
using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;
using HarmonyLib;
using UnityEngine;

namespace BetterBetterTeleporter;

public record struct ModConfigData
{
    public bool ResetCooldownOnOrbit;
    public int TeleporterCooldown;
    public bool IsTeleportKeep;
    public string TeleporterAlwaysKeep;
    public string TeleporterAlwaysDrop;
    public int InverseTeleporterCooldown;
    public bool IsInverseTeleportKeep;
    public string InverseTeleporterAlwaysKeep;
    public string InverseTeleporterAlwaysDrop;
    public float BatteryDrainPercent;
}

public enum ItemTeleportBehavior { Keep, Drop }

public class ModConfig : SyncedConfig2<ModConfig>
{
    [SyncedEntryField] public SyncedEntry<bool> ResetCooldownOnOrbit;
    [SyncedEntryField] public SyncedEntry<int> TeleporterCooldown;
    [SyncedEntryField] public SyncedEntry<ItemTeleportBehavior> TeleporterBehavior;
    [SyncedEntryField] public SyncedEntry<string> TeleporterAlwaysKeep;
    [SyncedEntryField] public SyncedEntry<string> TeleporterAlwaysDrop;
    [SyncedEntryField] public SyncedEntry<int> InverseTeleporterCooldown;
    [SyncedEntryField] public SyncedEntry<ItemTeleportBehavior> InverseTeleporterBehavior;
    [SyncedEntryField] public SyncedEntry<string> InverseTeleporterAlwaysKeep;
    [SyncedEntryField] public SyncedEntry<string> InverseTeleporterAlwaysDrop;
    [SyncedEntryField] public SyncedEntry<float> BatteryDrainPercent;

    public ModConfig(ConfigFile config) : base(PluginInfo.PLUGIN_GUID)
    {
        Plugin.Logger.LogDebug("Start config");
        config.SaveOnConfigSet = false;

        // # General
        Plugin.Logger.LogDebug("Bind general");
        ResetCooldownOnOrbit = config.BindSyncedEntry("General", "ResetCooldownOnOrbit", false, new ConfigDescription("Resets the cooldown time on teleporters between days."));

        // # Teleporter
        Plugin.Logger.LogDebug("Bind teleporter");
        TeleporterCooldown = config.BindSyncedEntry("Teleporter", "TeleporterCooldown", 10, new ConfigDescription("Cooldown time (in seconds) for using the Teleporter.", new AcceptableValueRange<int>(0, 600)));
        TeleporterBehavior = config.BindSyncedEntry("Teleporter", "TeleporterBehavior", ItemTeleportBehavior.Drop, new ConfigDescription("Makes the Teleporter \"Drop\" or \"Keep\" items on teleport."));
        TeleporterAlwaysKeep = config.BindSyncedEntry("Teleporter", "TeleporterAlwaysKeep", "", new ConfigDescription("Keep these items regardless of Teleporter behavior (comma-separated item names).\nDoes nothing if TeleporterBehavior is set to \"Keep\"."));
        TeleporterAlwaysDrop = config.BindSyncedEntry("Teleporter", "TeleporterAlwaysDrop", "", new ConfigDescription("Drop these items regardless of Teleporter behavior (comma-separated item names).\nDoes nothing if TeleporterBehavior is set to \"Drop\"."));

        // # Inverse Teleporter
        Plugin.Logger.LogDebug("Bind inverse teleporter");
        InverseTeleporterCooldown = config.BindSyncedEntry("Inverse Teleporter", "InverseTeleporterCooldown", 210, new ConfigDescription("Cooldown time (in seconds) for using the Inverse Teleporter.", new AcceptableValueRange<int>(0, 600)));
        InverseTeleporterBehavior = config.BindSyncedEntry("Inverse Teleporter", "InverseTeleporterBehavior", ItemTeleportBehavior.Drop, new ConfigDescription("Sets whether or not items are kept or dropped when using the Inverse Teleporter. Options: \"Drop\", \"Keep\"."));
        InverseTeleporterAlwaysKeep = config.BindSyncedEntry("Inverse Teleporter", "InverseTeleporterAlwaysKeep", "", new ConfigDescription("Keep these items regardless of Inverse Teleporter behavior (comma-separated item names).\nDoes nothing if InverseTeleporterBehavior is set to \"Keep\"."));
        InverseTeleporterAlwaysDrop = config.BindSyncedEntry("Inverse Teleporter", "InverseTeleporterAlwaysDrop", "", new ConfigDescription("Drop these items regardless of Inverse Teleporter behavior (comma-separated item names).\nDoes nothing if InverseTeleporterBehavior is set to \"Drop\"."));
        BatteryDrainPercent = config.BindSyncedEntry("Inverse Teleporter", "BatteryDrainPercent", 0.0f, new ConfigDescription("Drains all held battery items by a percentage when using the Inverse Teleporter. 0.0 means no drain. 1.0 means 100% drained.", new AcceptableValueRange<float>(0.0f, 1.0f)));

        ((Dictionary<ConfigDefinition, string>)AccessTools.Property(typeof(ConfigFile), "OrphanedEntries").GetValue(config)).Clear();
        config.Save();
        config.SaveOnConfigSet = true;
        Plugin.Logger.LogDebug("ConfigManager register");
        ConfigManager.Register(this);
    }
}