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

    public static ItemRule FromId(string id, List<ItemRule> except)
    {
        // Add new rule filters here
        switch (id)
        {
            case HeldItemFilter.Id: return new HeldItemFilter(except);
            case PocketedItemFilter.Id: return new PocketedItemFilter(except);
            case ScrapItemFilter.Id: return new ScrapItemFilter(except);
            case NonScrapItemFilter.Id: return new NonScrapItemFilter(except);
            case ValueItemFilter.Id: return new ValueItemFilter(except);
            case WorthlessItemFilter.Id: return new WorthlessItemFilter(except);
            case MetalItemFilter.Id: return new MetalItemFilter(except);
            case NonMetalItemFilter.Id: return new NonMetalItemFilter(except);
            case WeaponItemFilter.Id: return new WeaponItemFilter(except);
            case NonWeaponItemFilter.Id: return new NonWeaponItemFilter(except);
            case BatteryItemFilter.Id: return new BatteryItemFilter(except);
            case NonBatteryItemFilter.Id: return new NonBatteryItemFilter(except);
            case ChargedItemFilter.Id: return new ChargedItemFilter(except);
            case DischargedItemFilter.Id: return new DischargedItemFilter(except);
            case OneHandedItemFilter.Id: return new OneHandedItemFilter(except);
            case TwoHandedItemFilter.Id: return new TwoHandedItemFilter(except);
            case WeightedItemFilter.Id: return new WeightedItemFilter(except);
            case WeightlessItemFilter.Id: return new WeightlessItemFilter(except);
        }
        Plugin.Logger?.LogWarning($"Unknown item filter: {id}. Falling back to item name matching.");
        return new ItemNameRule(id);
    }
}

public abstract class ItemRule(string id)
{
    public abstract bool IsMatch(IPlayerInfo player, IItemInfo item);
    public override string ToString() => id;
}

public class ItemNameRule(string name) : ItemRule(name)
{
    const StringComparison caseInsensitive = StringComparison.OrdinalIgnoreCase;
    private readonly string name = name;
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (item.Name.Equals(name, caseInsensitive)) return true;        // ExtensionLadder
        if (item.TypeId.Equals(name, caseInsensitive)) return true;      // ExtensionLadderItem
        if (item.DisplayName.Equals(name, caseInsensitive)) return true; // Extension Ladder
        return false;
    }
}

public abstract class ItemFilter(string id, List<ItemRule> except) : ItemRule(id)
{
    public override bool IsMatch(IPlayerInfo player, IItemInfo item) => except.Count == 0 || !except.Any(rule => rule.IsMatch(player, item));
}

public class HeldItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "held";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.IsPocketed.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return !item.IsPocketed.Value && base.IsMatch(player, item);
    }
}

public class PocketedItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "pocketed";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.IsPocketed.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return item.IsPocketed.Value && base.IsMatch(player, item);
    }
}

public class ScrapItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "scrap";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.IsScrap.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return item.IsScrap.Value && base.IsMatch(player, item);
    }
}

public class NonScrapItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "nonscrap";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.IsScrap.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return !item.IsScrap.Value && base.IsMatch(player, item);
    }
}

public class ValueItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "value";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.IsScrap.HasValue || !item.Value.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return item.IsScrap.Value && item.Value.Value > 0 && base.IsMatch(player, item);
    }
}

public class WorthlessItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "worthless";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.IsScrap.HasValue || !item.Value.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return (!item.IsScrap.Value || item.Value.Value == 0) && base.IsMatch(player, item);
    }
}

public class MetalItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "metal";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.IsMetal.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return item.IsMetal.Value && base.IsMatch(player, item);
    }
}

public class NonMetalItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "nonmetal";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.IsMetal.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return !item.IsMetal.Value && base.IsMatch(player, item);
    }
}

public class WeaponItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "weapon";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.IsWeapon.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return item.IsWeapon.Value && base.IsMatch(player, item);
    }
}

public class NonWeaponItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "nonweapon";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.IsWeapon.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return !item.IsWeapon.Value && base.IsMatch(player, item);
    }
}

public class BatteryItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "battery";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.HasBattery.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return item.HasBattery.Value && base.IsMatch(player, item);
    }
}

public class NonBatteryItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "nonbattery";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.HasBattery.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return !item.HasBattery.Value && base.IsMatch(player, item);
    }
}

public class ChargedItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "charged";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.HasBattery.HasValue || !item.BatteryCharge.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return item.HasBattery.Value && item.BatteryCharge.Value > 0 && base.IsMatch(player, item);
    }
}

public class DischargedItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "discharged";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.HasBattery.HasValue || !item.BatteryCharge.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return item.HasBattery.Value && item.BatteryCharge.Value == 0 && base.IsMatch(player, item);
    }
}

public class OneHandedItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "onehanded";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.IsTwoHanded.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return !item.IsTwoHanded.Value && base.IsMatch(player, item);
    }
}

public class TwoHandedItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "twohanded";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.IsTwoHanded.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return item.IsTwoHanded.Value && base.IsMatch(player, item);
    }
}

public class WeightedItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "weighted";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.Weight.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return item.Weight.Value > 1 && base.IsMatch(player, item);
    }
}

public class WeightlessItemFilter(List<ItemRule> except) : ItemFilter(Id, except)
{
    public const string Id = "weightless";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (!item.Weight.HasValue)
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on GrabbableObject. Skipping filter.");
            return false;
        }
        return item.Weight.Value <= 1 && base.IsMatch(player, item);
    }
}