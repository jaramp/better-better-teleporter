using System.Collections.Generic;
using BetterBetterTeleporter.Networking;

namespace BetterBetterTeleporter.Utility;

public sealed class TeleporterConfigState
{
    public ItemTeleportBehavior Behavior { get; private set; }
    public List<ItemRule> Rules { get; private set; } = [];

    private readonly SyncedEntry<string> alwaysKeep;
    private readonly SyncedEntry<string> alwaysDrop;

    public TeleporterConfigState(SyncedEntry<ItemTeleportBehavior> behavior, SyncedEntry<string> alwaysKeep, SyncedEntry<string> alwaysDrop)
    {
        Behavior = behavior.Value;
        this.alwaysKeep = alwaysKeep;
        this.alwaysDrop = alwaysDrop;
        behavior.OnChanged += (o, n) => { Behavior = n; Rules = ComputeRules(); };
        alwaysKeep.OnChanged += (_, _) => { if (Behavior == ItemTeleportBehavior.Drop) Rules = ComputeRules(); };
        alwaysDrop.OnChanged += (_, _) => { if (Behavior == ItemTeleportBehavior.Keep) Rules = ComputeRules(); };
    }

    private List<ItemRule> ComputeRules()
    {
        return Behavior switch
        {
            ItemTeleportBehavior.Keep => ItemParser.ParseConfig(alwaysDrop.Value),
            ItemTeleportBehavior.Drop => ItemParser.ParseConfig(alwaysKeep.Value),
            _ => []
        };
    }
}
