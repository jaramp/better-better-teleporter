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
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(MessageName, (clientId, reader) => SyncedEntry.BroadcastAll());
        }
        else
        {
            SyncedEntry.BeginListenAll();
            using FastBufferWriter writer = new(0, Allocator.Temp);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(MessageName, 0ul, writer, NetworkDelivery.ReliableSequenced);
        }
    }

    [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect"), HarmonyPostfix]
    public static void PlayerLeave() => SyncedEntry.StopListenAll();
}