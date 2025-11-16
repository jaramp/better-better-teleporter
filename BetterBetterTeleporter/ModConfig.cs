using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using Mono.Cecil;
using Unity.Collections;
using Unity.Netcode;

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

        // ResetCooldownOnOrbit.Entry.SettingChanged += (o, e) => ResetCooldownOnOrbit.Sync();
        // TeleporterCooldown.Entry.SettingChanged += (o, e) => TeleporterCooldown.Sync();
        // TeleporterBehavior.Entry.SettingChanged += (o, e) => TeleporterBehavior.Sync();
        // TeleporterAlwaysKeep.Entry.SettingChanged += (o, e) => TeleporterAlwaysKeep.Sync();
        // TeleporterAlwaysDrop.Entry.SettingChanged += (o, e) => TeleporterAlwaysDrop.Sync();
        // InverseTeleporterCooldown.Entry.SettingChanged += (o, e) => InverseTeleporterCooldown.Sync();
        // InverseTeleporterBehavior.Entry.SettingChanged += (o, e) => InverseTeleporterBehavior.Sync();
        // InverseTeleporterAlwaysKeep.Entry.SettingChanged += (o, e) => InverseTeleporterAlwaysKeep.Sync();
        // InverseTeleporterAlwaysDrop.Entry.SettingChanged += (o, e) => InverseTeleporterAlwaysDrop.Sync();
        // BatteryDrainPercent.Entry.SettingChanged += (o, e) => BatteryDrainPercent.Sync();
    }
}

public interface ISyncable
{
    void Sync();
    void ListenForChange();
}

public class SyncedEntry<T>
{
    public ConfigEntry<T> Entry;
    private T _value;
    public T Value { get => _value; set { var old = _value; _value = value; OnChanged?.Invoke(old, value); } }
    public Action<T, T> OnChanged;

    private string MessageName => $"{PluginInfo.PLUGIN_GUID}.{Entry.Definition.Key}";
    private readonly Func<T, int> size;
    private readonly Func<FastBufferReader, T> read;
    private readonly Action<FastBufferWriter, T> write;

    public SyncedEntry(ConfigEntry<T> entry, Func<T, int> size, Func<FastBufferReader, T> read, Action<FastBufferWriter, T> write)
    {
        Entry = entry;
        Value = entry.Value;
        this.size = size;
        this.read = read;
        this.write = write;
        Entry.SettingChanged += (o, e) => Sync();
    }

    public void Sync()
    {
        Plugin.Logger.LogDebug($"Sync called on {Entry.Definition.Key}.");
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer) return;
        Plugin.Logger.LogDebug($"Syncing {Entry.Definition.Key}: {Value} to {Entry.Value}");
        Value = Entry.Value;
        Broadcast();
    }

    public void Broadcast()
    {
        Plugin.Logger.LogDebug($"Broadcast called on {Entry.Definition.Key}.");
        if (NetworkManager.Singleton.ConnectedClientsList.Count <= 1) return;

        using FastBufferWriter writer = new(size(Value), Allocator.Temp);
        write(writer, Value);

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId == NetworkManager.Singleton.LocalClientId) continue;
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(MessageName, client.ClientId, writer, NetworkDelivery.ReliableSequenced);
            Plugin.Logger.LogDebug($"Sent message {MessageName} to client {client.ClientId}.");
        }
    }

    public void BeginListening()
    {
        Plugin.Logger.LogDebug($"BeginListening called on {Entry.Definition.Key}.");
        if (NetworkManager.Singleton?.CustomMessagingManager == null) return;
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(MessageName, (clientId, reader) =>
        {
            if (!NetworkManager.Singleton || NetworkManager.Singleton.IsServer) return;
            Plugin.Logger.LogDebug($"Received message {MessageName} from client {clientId}.");
            Value = read(reader);
        });
    }

    public void StopListening(bool resetToLocalConfig = true)
    {
        Plugin.Logger.LogDebug($"StopListening called on {Entry.Definition.Key}.");
        if (resetToLocalConfig) Value = Entry.Value;
        if (NetworkManager.Singleton?.CustomMessagingManager == null) return;
        NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(MessageName);
    }
}

public static class SyncedEntry
{
    public static SyncedEntry<int> BindSynced(this ConfigFile config, string section, string key, int value, ConfigDescription description)
    {
        return new(config.Bind(section, key, value, description), _ => sizeof(int),
        reader => { reader.ReadValueSafe(out int result); return result; },
        (writer, value) => writer.WriteValueSafe(value));
    }

    public static SyncedEntry<float> BindSynced(this ConfigFile config, string section, string key, float value, ConfigDescription description)
    {
        return new(config.Bind(section, key, value, description), _ => sizeof(float),
        reader => { reader.ReadValueSafe(out float result); return result; },
        (writer, value) => writer.WriteValueSafe(value));
    }

    public static SyncedEntry<bool> BindSynced(this ConfigFile config, string section, string key, bool value, ConfigDescription description)
    {
        return new(config.Bind(section, key, value, description), _ => sizeof(bool),
        reader => { reader.ReadValueSafe(out bool result); return result; },
        (writer, value) => writer.WriteValueSafe(value));
    }

    public static SyncedEntry<string> BindSynced(this ConfigFile config, string section, string key, string value, ConfigDescription description)
    {
        return new(config.Bind(section, key, value, description), value => FastBufferWriter.GetWriteSize(value),
        reader => { reader.ReadValueSafe(out string result); return result; },
        (writer, value) => writer.WriteValueSafe(value));
    }

    public static SyncedEntry<T> BindSynced<T>(this ConfigFile config, string section, string key, T value, ConfigDescription description) where T : Enum
    {
        return new(config.Bind(section, key, value, description), _ => sizeof(int),
        reader => { reader.ReadValueSafe(out int result); return (T)(object)result; },
        (writer, value) => writer.WriteValueSafe(Convert.ToInt32(value)));
    }
}

[HarmonyPatch]
public static class PlayerConnectionSyncManager
{
    private const string MessageName = $"{PluginInfo.PLUGIN_GUID}.Connect";

    [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "ConnectClientToPlayerObject"), HarmonyPostfix]
    public static void ConnectClientToPlayerObject()
    {
        Plugin.Logger.LogDebug($"HostJoin()");
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(MessageName, (clientId, reader) =>
            {
                Plugin.Logger.LogDebug($"Received message {MessageName} from client {clientId}.");

                Plugin.ModConfig.ResetCooldownOnOrbit.Broadcast();
                Plugin.ModConfig.TeleporterCooldown.Broadcast();
                Plugin.ModConfig.TeleporterBehavior.Broadcast();
                Plugin.ModConfig.TeleporterAlwaysKeep.Broadcast();
                Plugin.ModConfig.TeleporterAlwaysDrop.Broadcast();
                Plugin.ModConfig.InverseTeleporterCooldown.Broadcast();
                Plugin.ModConfig.InverseTeleporterBehavior.Broadcast();
                Plugin.ModConfig.InverseTeleporterAlwaysKeep.Broadcast();
                Plugin.ModConfig.InverseTeleporterAlwaysDrop.Broadcast();
                Plugin.ModConfig.BatteryDrainPercent.Broadcast();
            });
        }
        else
        {
            Plugin.ModConfig.ResetCooldownOnOrbit.BeginListening();
            Plugin.ModConfig.TeleporterCooldown.BeginListening();
            Plugin.ModConfig.TeleporterBehavior.BeginListening();
            Plugin.ModConfig.TeleporterAlwaysKeep.BeginListening();
            Plugin.ModConfig.TeleporterAlwaysDrop.BeginListening();
            Plugin.ModConfig.InverseTeleporterCooldown.BeginListening();
            Plugin.ModConfig.InverseTeleporterBehavior.BeginListening();
            Plugin.ModConfig.InverseTeleporterAlwaysKeep.BeginListening();
            Plugin.ModConfig.InverseTeleporterAlwaysDrop.BeginListening();
            Plugin.ModConfig.BatteryDrainPercent.BeginListening();

            using FastBufferWriter writer = new(0, Allocator.Temp);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(MessageName, 0ul, writer, NetworkDelivery.ReliableSequenced);
        }
    }

    [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect"), HarmonyPostfix]
    public static void PlayerLeave()
    {
        Plugin.Logger.LogDebug($"PlayerLeave()");
        Plugin.ModConfig.ResetCooldownOnOrbit.StopListening();
        Plugin.ModConfig.TeleporterCooldown.StopListening();
        Plugin.ModConfig.TeleporterBehavior.StopListening();
        Plugin.ModConfig.TeleporterAlwaysKeep.StopListening();
        Plugin.ModConfig.TeleporterAlwaysDrop.StopListening();
        Plugin.ModConfig.InverseTeleporterCooldown.StopListening();
        Plugin.ModConfig.InverseTeleporterBehavior.StopListening();
        Plugin.ModConfig.InverseTeleporterAlwaysKeep.StopListening();
        Plugin.ModConfig.InverseTeleporterAlwaysDrop.StopListening();
        Plugin.ModConfig.BatteryDrainPercent.StopListening();
    }
}