using System;
using System.Diagnostics.CodeAnalysis;

namespace BetterBetterTeleporter.Adapters;

public interface IItem
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Match adapted name")]
    string name { get; }

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Match adapted name")]
    string itemName { get; }
}

public class ItemAdapter(Item source) : IItem
{
    public string name => source.name;
    public string itemName => source.itemName;
}

public static class ItemExtensions { extension(Item source) { public IItem Adapter => new ItemAdapter(source); } }