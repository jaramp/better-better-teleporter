using System;
using System.Collections.Generic;

namespace BetterBetterTeleporter.Utility;

public class TeleporterConfigState
{
    public readonly ItemTeleportBehavior Behavior;
    public readonly List<ItemRule> Rules;

    private static bool _init;
    private static readonly (Func<string> Raw, List<ItemRule> Rules)[,] _cache = new (Func<string>, List<ItemRule>)[2, 2];

    public TeleporterConfigState(bool inverse)
    {
        var config = Plugin.ModConfig;

        if (!_init)
        {
            _init = true;

            _cache[0, 0] = (() => config.TeleporterAlwaysKeep.Value, null);
            _cache[0, 1] = (() => config.TeleporterAlwaysDrop.Value, null);
            _cache[1, 0] = (() => config.InverseTeleporterAlwaysKeep.Value, null);
            _cache[1, 1] = (() => config.InverseTeleporterAlwaysDrop.Value, null);

            config.TeleporterAlwaysKeep.OnChanged += (o, n) => Reparse(ref _cache[0, 0]);
            config.TeleporterAlwaysDrop.OnChanged += (o, n) => Reparse(ref _cache[0, 1]);
            config.InverseTeleporterAlwaysKeep.OnChanged += (o, n) => Reparse(ref _cache[1, 0]);
            config.InverseTeleporterAlwaysDrop.OnChanged += (o, n) => Reparse(ref _cache[1, 1]);
        }
        Behavior = inverse ? config.InverseTeleporterBehavior.Value : config.TeleporterBehavior.Value;

        int inverseIndex = inverse ? 1 : 0;
        int behaviorIndex = Behavior == ItemTeleportBehavior.Keep ? 1 : 0;
        ref var entry = ref _cache[inverseIndex, behaviorIndex];
        Rules = entry.Rules ??= ItemParser.ParseConfig(entry.Raw());
    }

    private static void Reparse(ref (Func<string> Raw, List<ItemRule> Rules) entry) => entry.Rules = ItemParser.ParseConfig(entry.Raw());
}
