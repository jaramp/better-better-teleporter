using HarmonyLib;
using GameNetcodeStuff;
using Unity.Netcode;
using Unity.Collections;

namespace BetterBetterTeleporter.Patches;

/// <summary>
/// This patch class is responsible for synchronizing the configuration settings between the server and clients.
/// It uses Unity's NetworkManager to send and receive configuration data.
/// </summary>
[HarmonyPatch(typeof(PlayerControllerB))]
public class ConfigSyncPatch
{
    private const string ConfigSyncRequestName = "BetterBetterTeleporterConfigSyncRequest";
    private const string ConfigSyncReceiveName = "BetterBetterTeleporterConfigReceive";

    /// <summary>
    /// This Harmony Postfix patch runs after the ConnectClientToPlayerObject method.
    /// It initializes the local player and registers the necessary message handlers for config sync.
    /// </summary>
    [HarmonyPatch("ConnectClientToPlayerObject"), HarmonyPostfix]
    public static void InitializeLocalPlayer()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // Server host
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(ConfigSyncRequestName, OnReceiveConfigSyncRequest);
        }
        else
        {
            // Other players
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(ConfigSyncReceiveName, OnReceiveConfigSync);
            RequestConfigSync();
        }
    }

    /// <summary>
    /// Server host sends the configuration data to all connected clients.
    /// </summary>
    public static void SendConfigSyncToClients()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        TeleporterConfigData data = ConfigSettings.GetConfigDataFromEntries();
        using FastBufferWriter writer = new(1024, Allocator.Temp);
        writer.WriteValueSafe(data);

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId != NetworkManager.Singleton.LocalClientId)
            {
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(ConfigSyncReceiveName, client.ClientId, writer, NetworkDelivery.ReliableSequenced);
                Plugin.Logger.LogDebug($"Sent config sync to client {client.ClientId}");
            }
        }
    }

    /// <summary>
    /// Clients request the configuration data from the server.
    /// </summary>
    private static void RequestConfigSync()
    {
        if (!NetworkManager.Singleton.IsClient) return;

        Plugin.Logger.LogInfo("Sending config sync request to server.");

        using FastBufferWriter writer = new(0, Allocator.Temp);
        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(ConfigSyncRequestName, 0ul, writer, NetworkDelivery.ReliableSequenced);
    }

    /// <summary>
    /// Handles the config sync request received from a client.
    /// </summary>
    /// <param name="clientId">The ID of the client that sent the request.</param>
    /// <param name="reader">The FastBufferReader containing the request data.</param>
    private static void OnReceiveConfigSyncRequest(ulong clientId, FastBufferReader reader)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        Plugin.Logger.LogInfo($"Sync request from client {clientId}. Sending config sync.");
        TeleporterConfigData data = ConfigSettings.GetConfigDataFromEntries();
        using FastBufferWriter writer = new(1024, Allocator.Temp);
        writer.WriteValueSafe(data);

        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(ConfigSyncReceiveName, clientId, writer, NetworkDelivery.ReliableSequenced);
        Plugin.Logger.LogDebug($"Sent config sync to client {clientId}");
    }

    /// <summary>
    /// Handles the config sync data received from the server.
    /// </summary>
    /// <param name="clientId">The ID of the server that sent the config.</param>
    /// <param name="reader">The FastBufferReader containing the config data.</param>
    private static void OnReceiveConfigSync(ulong clientId, FastBufferReader reader)
    {
        if (NetworkManager.Singleton.IsServer) return;

        Plugin.Logger.LogInfo("Receiving sync from server.");
        var localReader = reader;
        try
        {
            localReader.ReadValueSafe(out TeleporterConfigData data);
            ConfigSettings.UpdateCurrentGameSettings(data);
            Plugin.Logger.LogInfo("Successfully applied synced config.");
        }
        catch (System.Exception e)
        {
            Plugin.Logger.LogError($"Error receiving config sync from server: {e}");
        }
    }
}
