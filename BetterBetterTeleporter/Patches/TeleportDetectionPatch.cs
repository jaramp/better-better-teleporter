using System.Collections.Generic;
using System.Reflection.Emit;
using GameNetcodeStuff;
using HarmonyLib;

namespace BetterBetterTeleporter.Patches;

/// <summary>
/// This is a patch to keep track of players that are currently being teleported.
/// Used by FixTeleporterBugsPatch.cs and KeepItemsOnTeleportPatch.cs
/// </summary>
[HarmonyPatch(typeof(ShipTeleporter))]
public static class TeleportDetectionPatch
{
    public static bool IsTeleporting(PlayerControllerB player)
    {
        if (!StartOfRound.Instance.ClientPlayerList.ContainsKey(player.actualClientId))
            return false; // Player is disconnecting

        if (IsRegularTeleporting()) return true;
        if (IsInverseTeleporting(player)) return true;

        return false; // Unknown, assume not teleporting
    }

    private static bool isTeleporting;

    [HarmonyPatch("beamUpPlayer", MethodType.Enumerator)]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> TeleporterTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var dropAllHeldItemsAndSyncMethod = AccessTools.Method(typeof(PlayerControllerB), "DropAllHeldItemsAndSync");
        var beforeMethod = AccessTools.Method(typeof(TeleportDetectionPatch), nameof(BeforeTeleporterDropAllHeldItemsAndSync));

        foreach (var instruction in instructions)
        {
            if (instruction.Calls(dropAllHeldItemsAndSyncMethod))
            {
                yield return new CodeInstruction(OpCodes.Call, beforeMethod);
            }
            yield return instruction;
        }
    }

    [HarmonyPatch("beamUpPlayer", MethodType.Enumerator)]
    [HarmonyFinalizer]
    static System.Exception TeleporterFinalizer(System.Exception __exception)
    {
        AfterTeleporterDropAllHeldItemsAndSync();
        return __exception;
    }

    public static void BeforeTeleporterDropAllHeldItemsAndSync() => isTeleporting = true;
    public static void AfterTeleporterDropAllHeldItemsAndSync() => isTeleporting = false;
    public static bool IsRegularTeleporting() => isTeleporting;

    private static readonly HashSet<int> InverseTeleportingPlayers = [];
    public static bool IsInverseTeleporting(PlayerControllerB player) => InverseTeleportingPlayers.Contains((int)player.playerClientId);
    [HarmonyPatch("TeleportPlayerOutWithInverseTeleporter"), HarmonyPrefix]
    public static void TeleportPlayerOutWithInverseTeleporterPrefix(int playerObj) => InverseTeleportingPlayers.Add(playerObj);
    [HarmonyPatch("TeleportPlayerOutWithInverseTeleporter"), HarmonyPostfix]
    public static void TeleportPlayerOutWithInverseTeleporterPostfix(int playerObj) => InverseTeleportingPlayers.Remove(playerObj);
}