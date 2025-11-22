using HarmonyLib;
using Unity.Collections;
using Unity.Netcode;

namespace BetterBetterTeleporter.Networking;

[HarmonyPatch]
public static class PlayerConnectionPatch
{
    private const string MessageName = $"{PluginInfo.PLUGIN_GUID}.Connect";

    [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "ConnectClientToPlayerObject"), HarmonyPostfix]
    public static void ConnectClientToPlayerObject()
    {
        var messager = NetworkManager.Singleton.CustomMessagingManager;
        if (NetworkManager.Singleton.IsServer)
        {
            SyncedEntries.ResetToLocalConfig();
            messager.RegisterNamedMessageHandler(MessageName, (clientId, reader) => SyncedEntries.SendAllToClient(clientId));
        }
        else
        {
            SyncedEntries.BeginListening();
            using FastBufferWriter writer = new(0, Allocator.Temp);
            messager.SendNamedMessage(MessageName, 0ul, writer, NetworkDelivery.ReliableSequenced);
        }
    }

    [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect"), HarmonyPostfix]
    public static void PlayerLeave() => SyncedEntries.StopListening();
}