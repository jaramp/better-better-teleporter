using BetterBetterTeleporter.Adapters;

namespace BetterBetterTeleporter.Tests.Fakes;

public sealed class FakeItemInfo : IItemInfo
{
    public string Name { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string TypeId { get; init; } = "";
}
