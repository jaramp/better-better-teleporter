using System.Collections;
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

        if (player.isPlayerDead || player.disableInteract)
            return false; // Player is incapacitated

        if (IsRegularTeleporting()) return true;
        if (IsInverseTeleporting(player)) return true;

        return false; // Unknown, assume not teleporting
    }

    private static bool isTeleporting;

    [HarmonyPatch("beamUpPlayer", MethodType.Enumerator)]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> TeleporterTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var beforeMethod = AccessTools.Method(typeof(TeleportDetectionPatch), nameof(BeforeTeleporterDropAllHeldItems));
        var code = new List<CodeInstruction>(instructions);
        int stateResetCount = 0;

        for (int i = 0; i < code.Count; i++)
        {
            var instruction = code[i];
            yield return instruction;
            if (instruction.opcode == OpCodes.Ldc_I4_M1 && i + 1 < code.Count && code[i + 1].opcode == OpCodes.Stfld)
            {
                stateResetCount++;
                if (stateResetCount == 3)
                {
                    yield return new CodeInstruction(OpCodes.Call, beforeMethod);
                }
            }
        }
    }

    public static void BeforeTeleporterDropAllHeldItems() => isTeleporting = true;
    public static void AfterTeleporterDropAllHeldItems() => isTeleporting = false;
    public static bool IsRegularTeleporting() => isTeleporting;

    private static readonly HashSet<int> InverseTeleportingPlayers = [];

    public static bool IsInverseTeleporting(PlayerControllerB player)
        => InverseTeleportingPlayers.Contains((int)player.playerClientId);

    [HarmonyPatch("TeleportPlayerOutWithInverseTeleporter")]
    [HarmonyPrefix]
    public static void TeleportPlayerOutWithInverseTeleporterPrefix(int playerObj)
        => InverseTeleportingPlayers.Add(playerObj);

    [HarmonyPatch("TeleportPlayerOutWithInverseTeleporter")]
    [HarmonyPostfix]
    public static void TeleportPlayerOutWithInverseTeleporterPostfix(int playerObj)
        => InverseTeleportingPlayers.Remove(playerObj);
}