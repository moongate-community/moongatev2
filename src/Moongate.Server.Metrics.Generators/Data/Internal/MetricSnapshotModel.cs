namespace Moongate.Server.Metrics.Generators.Data.Internal;

internal sealed class MetricSnapshotModel
{
    public MetricSnapshotModel(string typeName, string namespaceName, IReadOnlyList<MetricPropertyModel> properties)
    {
        TypeName = typeName;
        NamespaceName = namespaceName;
        Properties = properties;
    }

    public string TypeName { get; }

    public string NamespaceName { get; }

    public IReadOnlyList<MetricPropertyModel> Properties { get; }
}
