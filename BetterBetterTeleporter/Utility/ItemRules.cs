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

    public static ItemRule FromId(string id, ItemFilterList itemList)
    {
        // Add new rule filters here
        switch (id)
        {
            case AllFilter.Id: return new AllFilter(itemList);
            case NoneFilter.Id: return new NoneFilter(itemList);
            case HeldItemFilter.Id: return new HeldItemFilter(itemList);
            case PocketedItemFilter.Id: return new PocketedItemFilter(itemList);
            case ScrapItemFilter.Id: return new ScrapItemFilter(itemList);
            case NonScrapItemFilter.Id: return new NonScrapItemFilter(itemList);
            case ValueItemFilter.Id: return new ValueItemFilter(itemList);
            case WorthlessItemFilter.Id: return new WorthlessItemFilter(itemList);
            case MetalItemFilter.Id: return new MetalItemFilter(itemList);
            case NonMetalItemFilter.Id: return new NonMetalItemFilter(itemList);
            case WeaponItemFilter.Id: return new WeaponItemFilter(itemList);
            case NonWeaponItemFilter.Id: return new NonWeaponItemFilter(itemList);
            case BatteryItemFilter.Id: return new BatteryItemFilter(itemList);
            case NonBatteryItemFilter.Id: return new NonBatteryItemFilter(itemList);
            case ChargedItemFilter.Id: return new ChargedItemFilter(itemList);
            case DischargedItemFilter.Id: return new DischargedItemFilter(itemList);
            case OneHandedItemFilter.Id: return new OneHandedItemFilter(itemList);
            case TwoHandedItemFilter.Id: return new TwoHandedItemFilter(itemList);
            case WeightedItemFilter.Id: return new WeightedItemFilter(itemList);
            case WeightlessItemFilter.Id: return new WeightlessItemFilter(itemList);
            case GordionFilter.Id: return new GordionFilter(itemList);
            case OffGordionFilter.Id: return new OffGordionFilter(itemList);
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
        if (item.TypeName.Equals(name, caseInsensitive)) return true;    // ExtensionLadderItem
        if (item.DisplayName.Equals(name, caseInsensitive)) return true; // Extension Ladder
        return false;
    }
}

public record ItemFilterList
{
    public bool IsExclusive { get; set; }
    public List<ItemRule> Rules { get; set; }
    public ItemFilterList(bool isExclusive, List<ItemRule> rules)
    {
        IsExclusive = isExclusive;
        Rules = rules;
    }
}

public abstract class ItemFilter(string id, ItemFilterList itemList) : ItemRule(id)
{
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        if (itemList.Rules.Count == 0) return true;
        return itemList.IsExclusive ^ itemList.Rules.Any(rule => rule.IsMatch(player, item));
    }
}

public class AllFilter(ItemFilterList except) : ItemFilter(Id, except)
{
    public const string Id = "all";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        return base.IsMatch(player, item);
    }
}

public class NoneFilter(ItemFilterList except) : ItemFilter(Id, except)
{
    public const string Id = "none";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        return !base.IsMatch(player, item);
    }
}

public class HeldItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class PocketedItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class ScrapItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class NonScrapItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class ValueItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class WorthlessItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class MetalItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class NonMetalItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class WeaponItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class NonWeaponItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class BatteryItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class NonBatteryItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class ChargedItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class DischargedItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class OneHandedItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class TwoHandedItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class WeightedItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class WeightlessItemFilter(ItemFilterList except) : ItemFilter(Id, except)
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

public class GordionFilter(ItemFilterList except) : ItemFilter(Id, except)
{
    public const string Id = "gordion";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        try
        {
            return !StartOfRound.Instance.inShipPhase && StartOfRound.Instance.currentLevel.levelID == 3 && base.IsMatch(player, item);
        }
        catch
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on StartOfRound. Skipping filter.");
            return false;
        }
    }
}

public class OffGordionFilter(ItemFilterList except) : ItemFilter(Id, except)
{
    public const string Id = "offgordion";
    public override bool IsMatch(IPlayerInfo player, IItemInfo item)
    {
        try
        {
            return !StartOfRound.Instance.inShipPhase && StartOfRound.Instance.currentLevel.levelID != 3 && base.IsMatch(player, item);
        }
        catch
        {
            Plugin.Logger.LogWarning($"Unable to use filter [{Id}] due to read issues on StartOfRound. Skipping filter.");
            return false;
        }
    }
}