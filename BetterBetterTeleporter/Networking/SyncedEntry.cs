using System;
using BepInEx.Configuration;
using Unity.Collections;
using Unity.Netcode;

namespace BetterBetterTeleporter.Networking;

public static class SyncedEntry
{
    public static System.Collections.Generic.List<ISyncable> SyncedEntries { get; private set; } = [];

    public static void SyncAll() => SyncedEntries.ForEach(x => x.Sync());
    public static void BroadcastAll() => SyncedEntries.ForEach(x => x.Broadcast());
    public static void BeginListenAll() => SyncedEntries.ForEach(x => x.BeginListening());
    public static void StopListenAll() => SyncedEntries.ForEach(x => x.StopListening());

    private static SyncedEntry<T> Add<T>(SyncedEntry<T> item)
    {
        SyncedEntries.Add(item);
        return item;
    }

    public static SyncedEntry<int> BindSynced(this ConfigFile config, string section, string key, int value, ConfigDescription description)
    {
        return Add<int>(new(config.Bind(section, key, value, description), _ => sizeof(int),
        reader => { reader.ReadValueSafe(out int result); return result; },
        (writer, value) => writer.WriteValueSafe(value)));
    }

    public static SyncedEntry<float> BindSynced(this ConfigFile config, string section, string key, float value, ConfigDescription description)
    {
        return Add<float>(new(config.Bind(section, key, value, description), _ => sizeof(float),
        reader => { reader.ReadValueSafe(out float result); return result; },
        (writer, value) => writer.WriteValueSafe(value)));
    }

    public static SyncedEntry<bool> BindSynced(this ConfigFile config, string section, string key, bool value, ConfigDescription description)
    {
        return Add<bool>(new(config.Bind(section, key, value, description), _ => sizeof(bool),
        reader => { reader.ReadValueSafe(out bool result); return result; },
        (writer, value) => writer.WriteValueSafe(value)));
    }

    public static SyncedEntry<string> BindSynced(this ConfigFile config, string section, string key, string value, ConfigDescription description)
    {
        return Add<string>(new(config.Bind(section, key, value, description), value => FastBufferWriter.GetWriteSize(value),
        reader => { reader.ReadValueSafe(out string result); return result; },
        (writer, value) => writer.WriteValueSafe(value)));
    }

    public static SyncedEntry<T> BindSynced<T>(this ConfigFile config, string section, string key, T value, ConfigDescription description) where T : Enum
    {
        return Add<T>(new(config.Bind(section, key, value, description), _ => sizeof(int),
        reader => { reader.ReadValueSafe(out int result); return (T)(object)result; },
        (writer, value) => writer.WriteValueSafe(Convert.ToInt32(value))));
    }
}

public class SyncedEntry<T> : ISyncable
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
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer) return;
        Value = Entry.Value;
        Broadcast();
    }

    public void Broadcast()
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count <= 1) return;

        using FastBufferWriter writer = new(size(Value), Allocator.Temp);
        write(writer, Value);

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId == NetworkManager.Singleton.LocalClientId) continue;
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(MessageName, client.ClientId, writer, NetworkDelivery.ReliableSequenced);
        }
    }

    public void BeginListening()
    {
        if (NetworkManager.Singleton?.CustomMessagingManager == null) return;
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(MessageName, (clientId, reader) =>
        {
            if (!NetworkManager.Singleton || NetworkManager.Singleton.IsServer) return;
            Value = read(reader);
        });
    }

    public void StopListening(bool resetToLocalConfig = true)
    {
        if (resetToLocalConfig) Value = Entry.Value;
        if (NetworkManager.Singleton?.CustomMessagingManager == null) return;
        NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(MessageName);
    }
}
