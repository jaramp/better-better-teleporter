using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace BetterBetterTeleporter.Patches;

[HarmonyPatch(typeof(ShipTeleporter))]
public static class TeleportDeadBodiesPatch
{
    [HarmonyPatch("beamUpPlayer", MethodType.Enumerator)]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> TeleporterTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var code = new List<CodeInstruction>(instructions);

        int start = code.FindIndex(x => x.opcode == OpCodes.Ldfld && Equals(x.operand, AccessTools.Field(typeof(DeadBodyInfo), nameof(DeadBodyInfo.grabBodyObject))));
        int end = code.FindIndex(start + 1, x => (x.opcode == OpCodes.Call || x.opcode == OpCodes.Callvirt) && Equals(x.operand, AccessTools.Method(typeof(ShipTeleporter), "SetPlayerTeleporterId")));

        if (start < 0 || end < start) throw new Exception("TeleportDeadBodiesPatch: Failed to find beamUpPlayer instructions");

        var continueLabel = generator.DefineLabel();
        var skipLabel = generator.DefineLabel();
        var call = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TeleportDeadBodiesPatch), nameof(IsDeadBodyTeleportEnabled)));
        call.labels.AddRange(code[start].labels);
        code[start].labels = [continueLabel];
        code[end - 4].labels.Add(skipLabel);
        code.InsertRange(start, [call, new(OpCodes.Brtrue, continueLabel), new(OpCodes.Pop), new(OpCodes.Br, skipLabel)]);
        return code;
    }

    private static bool IsDeadBodyTeleportEnabled() => Plugin.ModConfig.TeleportDeadBodies.Value;
}