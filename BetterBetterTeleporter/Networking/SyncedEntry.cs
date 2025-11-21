using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace BetterBetterTeleporter.Networking;

public static class SyncedEntries
{
    private static byte _idGen = 0;
    private const string SyncMessage = $"{PluginInfo.PLUGIN_GUID}.ConfigSync";
    private static readonly Dictionary<byte, ISyncable> AllEntries = [];
    private static readonly HashSet<byte> UnsyncedEntries = [];

    private static SyncedEntry<T> Add<T>(SyncedEntry<T> item)
    {
        byte id = _idGen++;
        AllEntries.Add(id, item);
        item.Entry.SettingChanged += (o, e) => ConfigEntryChanged(id, item);
        return item;
    }

    private static void ConfigEntryChanged<T>(byte key, SyncedEntry<T> item)
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsConnectedClient) item.Value = item.Entry.Value;
        else if (NetworkManager.Singleton.IsServer) ScheduleBroadcastFor(key, item);
    }

    private static bool _isBroadcasting = false;
    private static void ScheduleBroadcastFor<T>(byte id, SyncedEntry<T> item)
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer) return;

        item.Value = item.Entry.Value;

        if (NetworkManager.Singleton.ConnectedClientsList.Count <= 1) return;

        UnsyncedEntries.Add(id);
        if (_isBroadcasting) return;
        _isBroadcasting = true;
        NetworkManager.Singleton.StartCoroutine(Broadcast());
    }

    private static IEnumerator Broadcast()
    {
        // Wait for any other config change events to propagate
        yield return new WaitForSecondsRealtime(0.05f);
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer || NetworkManager.Singleton.ConnectedClientsList.Count <= 1)
        {
            _isBroadcasting = false;
            yield break;
        }
        var payload = AllEntries.Where(x => UnsyncedEntries.Contains(x.Key)).ToArray();
        SendPayload(SyncMessage, payload, NetworkManager.Singleton.ConnectedClientsIds.ToArray()[1..]);
        UnsyncedEntries.Clear();
        _isBroadcasting = false;
    }

    public static void SendAllToClient(ulong clientId)
    {
        if (clientId == 0uL) return;
        SendPayload(SyncMessage, AllEntries, clientId);
    }

    private static void SendPayload(string messageName, ICollection<KeyValuePair<byte, ISyncable>> payload, params ulong[] clients)
    {
        const int maxPayloadSize = 1024;

        var chunks = new List<PayloadChunk>([new()]);
        foreach (var data in payload)
        {
            int dataSize = sizeof(byte) + data.Value.GetSize();
            if (dataSize + chunks[^1].Size > maxPayloadSize) chunks.Add(new());
            chunks[^1].Size += dataSize;
            chunks[^1].Items.Add(data);
        }

        foreach (var chunk in chunks)
        {
            // Write payload to writer
            using FastBufferWriter writer = new(chunk.Size, Allocator.Temp);
            foreach (var item in chunk.Items)
            {
                writer.WriteValueSafe(item.Key);
                item.Value.WriteToWriter(writer);
            }

            // Send message to clients
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(messageName, clients, writer, NetworkDelivery.ReliableSequenced);
        }
    }

    public static bool BeginListening()
    {
        if (NetworkManager.Singleton?.CustomMessagingManager == null) return false;
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(SyncMessage, ReadPayload);
        return true;
    }

    private static void ReadPayload(ulong clientId, FastBufferReader reader)
    {
        if (!NetworkManager.Singleton || NetworkManager.Singleton.IsServer) return;

        while (reader.TryBeginRead(sizeof(byte)))
        {
            reader.ReadByteSafe(out byte id);
            AllEntries[id].SetFromReader(reader);
        }
    }

    public static void ResetToLocalConfig()
    {
        foreach (var item in AllEntries.Values) item.ResetValue();
    }

    public static void StopListening(bool resetToLocalConfig = true)
    {
        if (resetToLocalConfig) ResetToLocalConfig();
        UnsyncedEntries.Clear();
        if (NetworkManager.Singleton?.CustomMessagingManager == null) return;
        NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(SyncMessage);
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
        return Add<string>(new(config.Bind(section, key, value, description), value => FastBufferWriter.GetWriteSize(value, true),
        reader => { reader.ReadValueSafe(out string result); return result; },
        (writer, value) => writer.WriteValueSafe(value, true)));
    }

    public static SyncedEntry<T> BindSynced<T>(this ConfigFile config, string section, string key, T value, ConfigDescription description) where T : Enum
    {
        return Add<T>(new(config.Bind(section, key, value, description), _ => sizeof(int),
        reader => { reader.ReadValueSafe(out int result); return (T)(object)result; },
        (writer, value) => writer.WriteValueSafe(Convert.ToInt32(value))));
    }

    private class PayloadChunk
    {
        public int Size;
        public ICollection<KeyValuePair<byte, ISyncable>> Items = [];
    }
}

public class SyncedEntry<T> : ISyncable
{
    private readonly Func<FastBufferReader, T> read;
    private readonly Action<FastBufferWriter, T> write;

    public ConfigEntry<T> Entry;
    public T Value { get => field; set { var old = field; field = value; OnChanged?.Invoke(old, value); } }
    public Action<T, T> OnChanged;

    public readonly Func<T, int> calcSize;


    public SyncedEntry(ConfigEntry<T> entry, Func<T, int> calcSize, Func<FastBufferReader, T> read, Action<FastBufferWriter, T> write)
    {
        Entry = entry;
        Value = entry.Value;
        this.calcSize = calcSize;
        this.read = read;
        this.write = write;
    }

    public int GetSize() => calcSize(Value);
    public void ResetValue() => Value = Entry.Value;
    public void SetFromReader(FastBufferReader reader) => Value = read(reader);
    public void WriteToWriter(FastBufferWriter writer) => write(writer, Value);
}
