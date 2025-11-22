using System;
using System.Collections.Generic;
using System.Linq;
using BetterBetterTeleporter.Adapters;

namespace BetterBetterTeleporter.Utility;

public static class ItemRules
{
    public static bool ShouldDropItem(this IPlayerInfo player, IItemInfo item, TeleporterConfigState state)
    {
        bool isDropDefault = state.Behavior == ItemTeleportBehavior.Drop;
        return player.ShouldDropItem(item, isDropDefault, state.Rules);
    }

    public static bool ShouldDropItem(this IPlayerInfo player, IItemInfo item, bool isDropDefault, List<ItemRule> rules)
    {
        return item != null && isDropDefault ^ rules.Any(rule => rule.IsMatch(player, item));
    }
}

public abstract class ItemRule(string id)
{
    public abstract bool IsMatch(IPlayerInfo player, IItemInfo item);
    public override string ToString() => id;
}

public class ItemNameRule(string name) : ItemRule(name)
{
    private readonly string name = name;
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        const StringComparison caseInsensitive = StringComparison.OrdinalIgnoreCase;
        if (item.Name.Equals(name, caseInsensitive)) return true;        // ExtensionLadder
        if (item.TypeId.Equals(name, caseInsensitive)) return true;      // ExtensionLadderItem
        if (item.DisplayName.Equals(name, caseInsensitive)) return true; // Extension Ladder
        return false;
    }
}

public abstract class RuleFilter(string id, List<ItemRule> except) : ItemRule(id)
{
    public override bool IsMatch(IPlayerInfo player, IItemInfo item) => except.Count == 0 || !except.Any(rule => rule.IsMatch(player, item));

    public static ItemRule FromId(string id, List<ItemRule> except)
    {
        switch (id)
        {
            case CurrentlyHeldFilter.Id: return new CurrentlyHeldFilter(except);
        }
        Plugin.Logger?.LogWarning($"Unknown item filter: {id}. Falling back to item name matching.");
        return new ItemNameRule(id);
    }
}

public class CurrentlyHeldFilter(List<ItemRule> except) : RuleFilter(Id, except)
{
    public const string Id = "current";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        var held = player.Slots[player.CurrentSlotIndex];
        return ReferenceEquals(held, item) && base.IsMatch(player, item);
    }
}
