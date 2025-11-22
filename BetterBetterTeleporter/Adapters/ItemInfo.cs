namespace BetterBetterTeleporter.Adapters;

public interface IItemInfo
{
    string Name { get; }
    string DisplayName { get; }
    string TypeId { get; }
}

public sealed class ItemInfo(GrabbableObject source) : IItemInfo
{
    public string Name => source.itemProperties.name;
    public string DisplayName => source.itemProperties.itemName;
    public string TypeId => source.GetType().Name;
}