namespace Moongate.UO.Data.Multi;

public static class MultiData
{
    public static Dictionary<int, MultiComponentList> Components { get; } = new();

    public static int Count => Components.Count;

    public static MultiComponentList GetComponents(int multiID)
        => Components.TryGetValue(multiID & 0x3FFF, out var mcl) ? mcl : MultiComponentList.Empty;
}
