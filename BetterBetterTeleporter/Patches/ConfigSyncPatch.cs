using HarmonyLib;
using GameNetcodeStuff;
using Unity.Netcode;
using Unity.Collections;

namespace BetterBetterTeleporter.Patches;

/// <summary>
/// This patch is responsible for synchronizing the configuration settings between the server and clients.
/// It uses Unity's NetworkManager to send and receive configuration data.
/// </summary>
[HarmonyPatch(typeof(PlayerControllerB))]
public static class ConfigSyncPatch
{
    private const string ConfigSyncRequestName = "BetterBetterTeleporterConfigRequest";
    private const string ConfigSyncReceiveName = "BetterBetterTeleporterConfigReceive";

    [HarmonyPatch("ConnectClientToPlayerObject"), HarmonyPostfix]
    public static void InitializeLocalPlayer()
    {
        if (NetworkManager.Singleton.IsServer) Host.Init();
        else Client.Init();
    }

    public static void SyncConfig()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        TeleporterConfigData data = ConfigSettings.GetConfigDataFromEntries();
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId == NetworkManager.Singleton.LocalClientId) continue;
            Host.SendConfigDataToClient(client.ClientId, data);
        }
    }

    private static class Host
    {
        public static void Init()
        {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(ConfigSyncRequestName, OnConfigDataRequest);
        }

        private static void OnConfigDataRequest(ulong clientId, FastBufferReader reader)
        {
            if (!NetworkManager.Singleton.IsServer) return;

            Plugin.Logger.LogInfo($"Sync request from client {clientId}. Sending config sync.");
            TeleporterConfigData data = ConfigSettings.GetConfigDataFromEntries();
            SendConfigDataToClient(clientId, data);
        }


        public static void SendConfigDataToClient(ulong clientId, TeleporterConfigData data)
        {
            using FastBufferWriter writer = new(1024, Allocator.Temp);
            writer.WriteValueSafe(data);

            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(ConfigSyncReceiveName, clientId, writer, NetworkDelivery.ReliableSequenced);
            Plugin.Logger.LogDebug($"Sent config sync to client {clientId}");
        }
    }

    private static class Client
    {
        public static void Init()
        {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(ConfigSyncReceiveName, Client.OnConfigDataReceived);
            RequestConfigData();
        }

        private static void RequestConfigData()
        {
            if (!NetworkManager.Singleton.IsClient) return;

            Plugin.Logger.LogInfo("Sending config sync request to server.");

            using FastBufferWriter writer = new(0, Allocator.Temp);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(ConfigSyncRequestName, 0ul, writer, NetworkDelivery.ReliableSequenced);
        }

        private static void OnConfigDataReceived(ulong clientId, FastBufferReader reader)
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
}

