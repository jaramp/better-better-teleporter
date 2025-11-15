using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using LethalNetworkAPI;
using LethalNetworkAPI.Utils;

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

public static class ModConfig
{
    // Saved Config Entries (Local to each player)
    public static ConfigEntry<bool> ResetCooldownOnOrbitEntry { get; private set; }
    public static ConfigEntry<int> TeleporterCooldownEntry { get; private set; }
    public static ConfigEntry<ItemTeleportBehavior> TeleporterBehaviorEntry { get; private set; }
    public static ConfigEntry<string> TeleporterAlwaysKeepEntry { get; private set; }
    public static ConfigEntry<string> TeleporterAlwaysDropEntry { get; private set; }
    public static ConfigEntry<int> InverseTeleporterCooldownEntry { get; private set; }
    public static ConfigEntry<ItemTeleportBehavior> InverseTeleporterBehaviorEntry { get; private set; }
    public static ConfigEntry<string> InverseTeleporterAlwaysKeepEntry { get; private set; }
    public static ConfigEntry<string> InverseTeleporterAlwaysDropEntry { get; private set; }
    public static ConfigEntry<float> BatteryDrainPercentEntry { get; private set; }

    // Current Game Settings (Synced from Host) - Using LNetworkVariable for automatic sync
    public static ModConfigData CurrentSettings => configSync.Value;
    private static LNetworkVariable<ModConfigData> configSync;

    public static event Action<ModConfigData> OnCooldownSettingsChanged;

    public static void Init(ConfigFile config)
    {
        config.SaveOnConfigSet = false;

        // # General
        ResetCooldownOnOrbitEntry = config.Bind("General", "ResetCooldownOnOrbit", false, new ConfigDescription("Resets the cooldown time on teleporters between days."));

        // # Teleporter
        TeleporterCooldownEntry = config.Bind("Teleporter", "TeleporterCooldown", 10, new ConfigDescription("Cooldown time (in seconds) for using the Teleporter.", new AcceptableValueRange<int>(0, 600)));
        TeleporterBehaviorEntry = config.Bind("Teleporter", "TeleporterBehavior", ItemTeleportBehavior.Drop, new ConfigDescription("Makes the Teleporter \"Drop\" or \"Keep\" items on teleport."));
        TeleporterAlwaysKeepEntry = config.Bind("Teleporter", "TeleporterAlwaysKeep", "", new ConfigDescription("Keep these items regardless of Teleporter behavior (comma-separated item names).\nDoes nothing if TeleporterBehavior is set to \"Keep\"."));
        TeleporterAlwaysDropEntry = config.Bind("Teleporter", "TeleporterAlwaysDrop", "", new ConfigDescription("Drop these items regardless of Teleporter behavior (comma-separated item names).\nDoes nothing if TeleporterBehavior is set to \"Drop\"."));

        // # Inverse Teleporter
        InverseTeleporterCooldownEntry = config.Bind("Inverse Teleporter", "InverseTeleporterCooldown", 210, new ConfigDescription("Cooldown time (in seconds) for using the Inverse Teleporter.", new AcceptableValueRange<int>(0, 600)));
        InverseTeleporterBehaviorEntry = config.Bind("Inverse Teleporter", "InverseTeleporterBehavior", ItemTeleportBehavior.Drop, new ConfigDescription("Sets whether or not items are kept or dropped when using the Inverse Teleporter. Options: \"Drop\", \"Keep\"."));
        InverseTeleporterAlwaysKeepEntry = config.Bind("Inverse Teleporter", "InverseTeleporterAlwaysKeep", "", new ConfigDescription("Keep these items regardless of Inverse Teleporter behavior (comma-separated item names).\nDoes nothing if InverseTeleporterBehavior is set to \"Keep\"."));
        InverseTeleporterAlwaysDropEntry = config.Bind("Inverse Teleporter", "InverseTeleporterAlwaysDrop", "", new ConfigDescription("Drop these items regardless of Inverse Teleporter behavior (comma-separated item names).\nDoes nothing if InverseTeleporterBehavior is set to \"Drop\"."));
        BatteryDrainPercentEntry = config.Bind("Inverse Teleporter", "BatteryDrainPercent", 0.0f, new ConfigDescription("Drains all held battery items by a percentage when using the Inverse Teleporter. 0.0 means no drain. 1.0 means 100% drained.", new AcceptableValueRange<float>(0.0f, 1.0f)));

        ((Dictionary<ConfigDefinition, string>)AccessTools.Property(typeof(ConfigFile), "OrphanedEntries").GetValue(config)).Clear();
        config.Save();
        configSync = LNetworkVariable<ModConfigData>.Connect("BetterBetterTeleporter.Config", ReadConfig(), onValueChanged: OnConfigSynced);
        SubscribeConfigChangeEvents();
    }

    private static void OnConfigSynced(ModConfigData oldValue, ModConfigData newValue)
    {
        OnCooldownSettingsChanged?.Invoke(newValue);
    }

    private static void SubscribeConfigChangeEvents()
    {
        // When the host updates their config, update the synced variable
        static void OnConfigChanged(object o, EventArgs e)
        {
            if (!LNetworkUtils.IsHostOrServer) return;
            configSync.Value = ReadConfig();
        }

        // Fire event to immediately update existing Teleporter cooldowns
        static void OnTeleporterCooldownChanged(object o, EventArgs e)
        {
            if (!LNetworkUtils.IsHostOrServer) return;
            OnConfigChanged(o, e);
            OnCooldownSettingsChanged?.Invoke(CurrentSettings);
        }

        TeleporterCooldownEntry.SettingChanged += OnTeleporterCooldownChanged;
        InverseTeleporterCooldownEntry.SettingChanged += OnTeleporterCooldownChanged;
        ResetCooldownOnOrbitEntry.SettingChanged += OnConfigChanged;
        BatteryDrainPercentEntry.SettingChanged += OnConfigChanged;
        TeleporterBehaviorEntry.SettingChanged += OnConfigChanged;
        TeleporterAlwaysKeepEntry.SettingChanged += OnConfigChanged;
        TeleporterAlwaysDropEntry.SettingChanged += OnConfigChanged;
        InverseTeleporterBehaviorEntry.SettingChanged += OnConfigChanged;
        InverseTeleporterAlwaysKeepEntry.SettingChanged += OnConfigChanged;
        InverseTeleporterAlwaysDropEntry.SettingChanged += OnConfigChanged;
    }

    private static ModConfigData ReadConfig() => new()
    {
        ResetCooldownOnOrbit = ResetCooldownOnOrbitEntry.Value,
        TeleporterCooldown = TeleporterCooldownEntry.Value,
        IsTeleportKeep = TeleporterBehaviorEntry.Value == ItemTeleportBehavior.Keep,
        TeleporterAlwaysKeep = TeleporterAlwaysKeepEntry.Value,
        TeleporterAlwaysDrop = TeleporterAlwaysDropEntry.Value,
        InverseTeleporterCooldown = InverseTeleporterCooldownEntry.Value,
        IsInverseTeleportKeep = InverseTeleporterBehaviorEntry.Value == ItemTeleportBehavior.Keep,
        InverseTeleporterAlwaysKeep = InverseTeleporterAlwaysKeepEntry.Value,
        InverseTeleporterAlwaysDrop = InverseTeleporterAlwaysDropEntry.Value,
        BatteryDrainPercent = BatteryDrainPercentEntry.Value,
    };
}