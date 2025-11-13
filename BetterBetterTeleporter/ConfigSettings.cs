using System;
using BepInEx.Configuration;
using Unity.Netcode;

using BetterBetterTeleporter.Patches;

namespace BetterBetterTeleporter;

public static class ConfigSettings
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

    // Current Game Settings (Synced from Host)
    public static TeleporterConfigData CurrentSettings = new();

    public static event Action OnCooldownSettingsChanged;

    public static void Init(ConfigFile config)
    {
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

        SubscribeOnChangeEvents();
        UpdateCurrentGameSettings(GetConfigDataFromEntries());
    }

    private static void SubscribeOnChangeEvents()
    {
        TeleporterCooldownEntry.SettingChanged += OnConfigChanged;
        InverseTeleporterCooldownEntry.SettingChanged += OnConfigChanged;
        ResetCooldownOnOrbitEntry.SettingChanged += OnConfigChanged;
        BatteryDrainPercentEntry.SettingChanged += OnConfigChanged;
        TeleporterBehaviorEntry.SettingChanged += OnConfigChanged;
        TeleporterAlwaysKeepEntry.SettingChanged += OnConfigChanged;
        TeleporterAlwaysDropEntry.SettingChanged += OnConfigChanged;
        InverseTeleporterBehaviorEntry.SettingChanged += OnConfigChanged;
        InverseTeleporterAlwaysKeepEntry.SettingChanged += OnConfigChanged;
        InverseTeleporterAlwaysDropEntry.SettingChanged += OnConfigChanged;
    }

    private static void OnConfigChanged(object sender, EventArgs args)
    {
        // When the host updates their config, it syncs across clients for the current game
        // When a client updates their config, it doesn't affect the current game settings (i.e., no sync)
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            UpdateCurrentGameSettings(GetConfigDataFromEntries());
            ConfigSyncPatch.SyncConfig();
        }
    }

    public static void UpdateCurrentGameSettings(TeleporterConfigData data)
    {
        var shouldNotifyCooldowns = HasChangedCooldowns(CurrentSettings, data);
        CurrentSettings = data;
        if (shouldNotifyCooldowns) OnCooldownSettingsChanged?.Invoke();
    }

    private static bool HasChangedCooldowns(TeleporterConfigData a, TeleporterConfigData b)
    {
        if (a.TeleporterCooldown != b.TeleporterCooldown) return true;
        if (a.InverseTeleporterCooldown != b.InverseTeleporterCooldown) return true;
        return false;
    }

    public static TeleporterConfigData GetConfigDataFromEntries()
    {
        return new TeleporterConfigData
        {
            TeleporterCooldown = TeleporterCooldownEntry.Value,
            InverseTeleporterCooldown = InverseTeleporterCooldownEntry.Value,
            ResetCooldownOnOrbit = ResetCooldownOnOrbitEntry.Value,
            BatteryDrainPercent = BatteryDrainPercentEntry.Value,
            TeleporterBehavior = TeleporterBehaviorEntry.Value,
            TeleporterAlwaysKeep = TeleporterAlwaysKeepEntry.Value,
            TeleporterAlwaysDrop = TeleporterAlwaysDropEntry.Value,
            InverseTeleporterBehavior = InverseTeleporterBehaviorEntry.Value,
            InverseTeleporterAlwaysKeep = InverseTeleporterAlwaysKeepEntry.Value,
            InverseTeleporterAlwaysDrop = InverseTeleporterAlwaysDropEntry.Value,
        };
    }
}

public struct TeleporterConfigData : INetworkSerializable
{
    public bool ResetCooldownOnOrbit;
    public int TeleporterCooldown;
    public ItemTeleportBehavior TeleporterBehavior;
    public string TeleporterAlwaysKeep;
    public string TeleporterAlwaysDrop;
    public int InverseTeleporterCooldown;
    public ItemTeleportBehavior InverseTeleporterBehavior;
    public string InverseTeleporterAlwaysKeep;
    public string InverseTeleporterAlwaysDrop;
    public float BatteryDrainPercent;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref TeleporterCooldown);
        serializer.SerializeValue(ref InverseTeleporterCooldown);
        serializer.SerializeValue(ref ResetCooldownOnOrbit);
        serializer.SerializeValue(ref BatteryDrainPercent);
        serializer.SerializeValue(ref TeleporterBehavior);
        serializer.SerializeValue(ref TeleporterAlwaysKeep);
        serializer.SerializeValue(ref TeleporterAlwaysDrop);
        serializer.SerializeValue(ref InverseTeleporterBehavior);
        serializer.SerializeValue(ref InverseTeleporterAlwaysKeep);
        serializer.SerializeValue(ref InverseTeleporterAlwaysDrop);
    }
}

public enum ItemTeleportBehavior { Keep, Drop }
