using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GameNetcodeStuff;

namespace BetterBetterTeleporter.Adapters;

public interface IPlayerControllerB
{
    IGrabbableObject[] ItemSlots { get; }

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Match adapted name")]
    int currentItemSlot { get; set; }
}

public class PlayerControllerBAdapter(PlayerControllerB source) : IPlayerControllerB
{
    public IGrabbableObject[] ItemSlots => [.. source.ItemSlots.Select(x => new GrabbableObjectAdapter(x))];

    public int currentItemSlot { get => source.currentItemSlot; set => source.currentItemSlot = value; }
}

public static class PlayerControllerBExtensions { extension(PlayerControllerB source) { public IPlayerControllerB Adapter => new PlayerControllerBAdapter(source); } }