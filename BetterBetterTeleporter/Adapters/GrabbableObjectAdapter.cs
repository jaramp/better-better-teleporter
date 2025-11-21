using System.Diagnostics.CodeAnalysis;

namespace BetterBetterTeleporter.Adapters;

public interface IGrabbableObject
{

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Match adapted name")]
    IItem itemProperties { get; }
}

public class GrabbableObjectAdapter(GrabbableObject source) : IGrabbableObject
{
    public IItem itemProperties => new ItemAdapter(source.itemProperties);
}


public static class GrabbableObjectExtensions { extension(GrabbableObject source) { public IGrabbableObject Adapter => new GrabbableObjectAdapter(source); } }