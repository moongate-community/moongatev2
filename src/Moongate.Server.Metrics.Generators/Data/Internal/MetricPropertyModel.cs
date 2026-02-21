namespace Moongate.Server.Metrics.Generators.Data.Internal;

internal sealed class MetricPropertyModel
{
    public MetricPropertyModel(
        string name,
        string propertyName,
        bool isNumeric,
        bool isBoolean,
        bool isEnum,
        bool isTimeSpan,
        bool isDateTimeOffset,
        bool isNullableDateTimeOffset,
        string transformTypeName,
        IReadOnlyList<string> aliases
    )
    {
        Name = name;
        PropertyName = propertyName;
        IsNumeric = isNumeric;
        IsBoolean = isBoolean;
        IsEnum = isEnum;
        IsTimeSpan = isTimeSpan;
        IsDateTimeOffset = isDateTimeOffset;
        IsNullableDateTimeOffset = isNullableDateTimeOffset;
        TransformTypeName = transformTypeName;
        Aliases = aliases;
    }

    public string Name { get; }

    public string PropertyName { get; }

    public bool IsNumeric { get; }

    public bool IsBoolean { get; }

    public bool IsEnum { get; }

    public bool IsTimeSpan { get; }

    public bool IsDateTimeOffset { get; }

    public bool IsNullableDateTimeOffset { get; }

    public string TransformTypeName { get; }

    public IReadOnlyList<string> Aliases { get; }
}
