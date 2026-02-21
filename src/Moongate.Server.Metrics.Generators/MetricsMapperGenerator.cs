using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Moongate.Server.Metrics.Generators.Data.Internal;

namespace Moongate.Server.Metrics.Generators;

[Generator]
public sealed class MetricsMapperGenerator : IIncrementalGenerator
{
    private const string MetricAttributeName = "Moongate.Server.Metrics.Data.Attributes.MetricAttribute";
    private const string MetricValueTransformTypeName = "Moongate.Server.Metrics.Data.Types.MetricValueTransformType";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var metricProperties = context.SyntaxProvider
                                      .CreateSyntaxProvider(
                                          static (node, _) => node is AttributeSyntax,
                                          static (syntaxContext, _) => CreateModel(syntaxContext)
                                      )
                                      .Where(static model => model is not null)
                                      .Collect();

        context.RegisterSourceOutput(
            metricProperties,
            static (productionContext, models) =>
            {
                var snapshots = models
                                .Where(static model => model is not null)
                                .Cast<MetricSnapshotModel>()
                                .GroupBy(static model => model.TypeName, StringComparer.Ordinal)
                                .Select(
                                    static group =>
                                    {
                                        var first = group.First();
                                        var properties = group
                                                         .SelectMany(static s => s.Properties)
                                                         .GroupBy(static p => p.PropertyName, StringComparer.Ordinal)
                                                         .Select(static g => g.First())
                                                         .OrderBy(static p => p.PropertyName, StringComparer.Ordinal)
                                                         .ToArray();

                                        return new MetricSnapshotModel(first.TypeName, first.NamespaceName, properties);
                                    }
                                )
                                .OrderBy(static model => model.TypeName, StringComparer.Ordinal)
                                .ToArray();

                var source = BuildSource(snapshots);
                productionContext.AddSource("MetricSnapshotMappers.Generated.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        );
    }

    private static MetricSnapshotModel? CreateModel(GeneratorSyntaxContext context)
    {
        if (context.Node is not AttributeSyntax attributeSyntax)
        {
            return null;
        }

        if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeCtor)
        {
            return null;
        }

        if (!string.Equals(attributeCtor.ContainingType.ToDisplayString(), MetricAttributeName, StringComparison.Ordinal))
        {
            return null;
        }

        var propertySymbol = ResolveTargetPropertySymbol(context, attributeSyntax);

        if (propertySymbol is null || propertySymbol.ContainingType is not { } containingType)
        {
            return null;
        }

        var attributeData = propertySymbol
                            .GetAttributes()
                            .FirstOrDefault(
                                a => string.Equals(
                                    a.AttributeClass?.ToDisplayString(),
                                    MetricAttributeName,
                                    StringComparison.Ordinal
                                )
                            );

        if (attributeData is null
            || attributeData.ConstructorArguments.Length == 0
            || attributeData.ConstructorArguments[0].Value is not string metricName)
        {
            return null;
        }

        var aliases = Array.Empty<string>();
        var transformTypeName = MetricValueTransformTypeName + ".None";

        foreach (var pair in attributeData.NamedArguments)
        {
            if (pair.Key == "Aliases")
            {
                aliases = pair.Value.Values
                              .Where(static v => v.Value is string)
                              .Select(static v => (string)v.Value!)
                              .Where(static s => !string.IsNullOrWhiteSpace(s))
                              .Distinct(StringComparer.Ordinal)
                              .ToArray();
            }

            if (pair.Key == "Transform" && pair.Value.Value is not null)
            {
                transformTypeName = GetTransformTypeName(pair.Value);
            }
        }

        var property = new MetricPropertyModel(
            metricName,
            propertySymbol.Name,
            IsNumericType(propertySymbol.Type),
            propertySymbol.Type.SpecialType == SpecialType.System_Boolean,
            propertySymbol.Type.TypeKind == TypeKind.Enum,
            IsTimeSpanType(propertySymbol.Type),
            IsDateTimeOffsetType(propertySymbol.Type),
            IsNullableDateTimeOffsetType(propertySymbol.Type),
            transformTypeName,
            aliases
        );

        return new MetricSnapshotModel(
            containingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", ""),
            containingType.ContainingNamespace.ToDisplayString(),
            new[] { property }
        );
    }

    private static IPropertySymbol? ResolveTargetPropertySymbol(GeneratorSyntaxContext context, AttributeSyntax attributeSyntax)
    {
        var targetNode = attributeSyntax.Parent?.Parent;

        return targetNode switch
        {
            PropertyDeclarationSyntax propertyDeclaration => context.SemanticModel.GetDeclaredSymbol(propertyDeclaration) as IPropertySymbol,
            ParameterSyntax parameterSyntax => ResolveRecordPropertyFromParameter(
                context.SemanticModel.GetDeclaredSymbol(parameterSyntax) as IParameterSymbol
            ),
            _ => null
        };
    }

    private static IPropertySymbol? ResolveRecordPropertyFromParameter(IParameterSymbol? parameter)
    {
        if (parameter?.ContainingType is not { } containingType)
        {
            return null;
        }

        return containingType
               .GetMembers(parameter.Name)
               .OfType<IPropertySymbol>()
               .FirstOrDefault(property => string.Equals(property.Name, parameter.Name, StringComparison.Ordinal));
    }

    private static string BuildSource(IReadOnlyList<MetricSnapshotModel> snapshots)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#pragma warning disable CS1591");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using Moongate.Server.Metrics.Data;");
        sb.AppendLine();
        sb.AppendLine("namespace Moongate.Server.Metrics.Data;");
        sb.AppendLine();
        sb.AppendLine("public static class MetricSnapshotMapperExtensions");
        sb.AppendLine("{");

        foreach (var snapshot in snapshots)
        {
            sb.Append("    public static IReadOnlyList<MetricSample> ToMetricSamples(this ");
            sb.Append(snapshot.TypeName);
            sb.AppendLine(" snapshot)");
            sb.AppendLine("    {");
            sb.AppendLine("        return");
            sb.AppendLine("        [");

            foreach (var property in snapshot.Properties)
            {
                var valueExpression = BuildValueExpression(property);

                if (valueExpression is null)
                {
                    continue;
                }

                sb.Append("            new(\"");
                sb.Append(Escape(property.Name));
                sb.Append("\", ");
                sb.Append(valueExpression);
                sb.AppendLine("),");

                foreach (var alias in property.Aliases)
                {
                    sb.Append("            new(\"");
                    sb.Append(Escape(alias));
                    sb.Append("\", ");
                    sb.Append(valueExpression);
                    sb.AppendLine("),");
                }
            }

            sb.AppendLine("        ];");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        sb.AppendLine("}");
        sb.AppendLine("#pragma warning restore CS1591");
        return sb.ToString();
    }

    private static string? BuildValueExpression(MetricPropertyModel property)
    {
        var propertyAccess = "snapshot." + property.PropertyName;

        return property.TransformTypeName switch
        {
            MetricValueTransformTypeName + ".TimeSpanMilliseconds" => property.IsTimeSpan
                ? propertyAccess + ".TotalMilliseconds"
                : null,
            MetricValueTransformTypeName + ".UnixTimeMillisecondsOrZero" => property.IsNullableDateTimeOffset
                ? propertyAccess + "?.ToUnixTimeMilliseconds() ?? 0d"
                : property.IsDateTimeOffset
                    ? propertyAccess + ".ToUnixTimeMilliseconds()"
                    : null,
            _ => property.IsBoolean
                ? "(" + propertyAccess + " ? 1d : 0d)"
                : property.IsNumeric || property.IsEnum
                    ? "(double)" + propertyAccess
                    : null
        };
    }

    private static bool IsNumericType(ITypeSymbol typeSymbol)
        => typeSymbol.SpecialType is SpecialType.System_Byte
            or SpecialType.System_SByte
            or SpecialType.System_Int16
            or SpecialType.System_UInt16
            or SpecialType.System_Int32
            or SpecialType.System_UInt32
            or SpecialType.System_Int64
            or SpecialType.System_UInt64
            or SpecialType.System_Single
            or SpecialType.System_Double
            or SpecialType.System_Decimal;

    private static bool IsTimeSpanType(ITypeSymbol typeSymbol)
        => string.Equals(typeSymbol.ToDisplayString(), "System.TimeSpan", StringComparison.Ordinal);

    private static bool IsDateTimeOffsetType(ITypeSymbol typeSymbol)
        => string.Equals(typeSymbol.ToDisplayString(), "System.DateTimeOffset", StringComparison.Ordinal);

    private static bool IsNullableDateTimeOffsetType(ITypeSymbol typeSymbol)
        => typeSymbol is INamedTypeSymbol namedType
            && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
            && namedType.TypeArguments.Length == 1
            && string.Equals(namedType.TypeArguments[0].ToDisplayString(), "System.DateTimeOffset", StringComparison.Ordinal);

    private static string Escape(string value)
        => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string GetTransformTypeName(TypedConstant transformConstant)
    {
        if (transformConstant.Value is int intValue)
        {
            return intValue switch
            {
                1 => MetricValueTransformTypeName + ".TimeSpanMilliseconds",
                2 => MetricValueTransformTypeName + ".UnixTimeMillisecondsOrZero",
                _ => MetricValueTransformTypeName + ".None"
            };
        }

        if (transformConstant.Value is string enumName && !string.IsNullOrWhiteSpace(enumName))
        {
            return MetricValueTransformTypeName + "." + enumName;
        }

        return MetricValueTransformTypeName + ".None";
    }
}
