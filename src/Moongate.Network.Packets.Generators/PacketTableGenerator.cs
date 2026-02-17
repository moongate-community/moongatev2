using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Moongate.Network.Packets.Generators;

[Generator]
public sealed class PacketTableGenerator : IIncrementalGenerator
{
    private const string PacketHandlerAttributeName = "Moongate.Network.Packets.Attributes.PacketHandlerAttribute";
    private const string PacketSizingTypeName = "Moongate.Network.Packets.Types.Packets.PacketSizing";

    private sealed class PacketModel
    {
        public string TypeName { get; }
        public byte OpCode { get; }
        public bool IsFixed { get; }
        public int Length { get; }
        public string? Description { get; }

        public PacketModel(string typeName, byte opCode, bool isFixed, int length, string? description)
        {
            TypeName = typeName;
            OpCode = opCode;
            IsFixed = isFixed;
            Length = length;
            Description = description;
        }
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var packetCandidates = context.SyntaxProvider.ForAttributeWithMetadataName(
            PacketHandlerAttributeName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (syntaxContext, _) => CreatePacketModel(syntaxContext)
        );

        var packetList = packetCandidates
                         .Where(static model => model is not null)
                         .Collect();

        context.RegisterSourceOutput(
            packetList,
            static (productionContext, packets) =>
            {
                var models = packets
                             .Where(static p => p is not null)
                             .Cast<PacketModel>()
                             .OrderBy(static p => p.OpCode)
                             .ToArray();

                var source = BuildSource(models);
                productionContext.AddSource("PacketTable.Generated.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        );
    }

    private static string BuildSource(IReadOnlyList<PacketModel> models)
    {
        var sb = new StringBuilder();

        sb.AppendLine("namespace Moongate.Network.Packets.Registry;");
        sb.AppendLine();
        sb.AppendLine("public static partial class PacketTable");
        sb.AppendLine("{");
        sb.AppendLine("    static partial void RegisterGenerated(PacketRegistry registry)");
        sb.AppendLine("    {");

        foreach (var model in models)
        {
            if (model.IsFixed)
            {
                if (model.Length <= 0)
                {
                    continue;
                }

                sb.Append("        registry.RegisterFixed<");
                sb.Append(model.TypeName);
                sb.Append(">(0x");
                sb.Append(model.OpCode.ToString("X2", CultureInfo.InvariantCulture));
                sb.Append(", ");
                sb.Append(model.Length.ToString(CultureInfo.InvariantCulture));
                if (!string.IsNullOrWhiteSpace(model.Description))
                {
                    sb.Append(", \"");
                    sb.Append(EscapeStringLiteral(model.Description));
                    sb.Append('"');
                }
                sb.AppendLine(");");
            }
            else
            {
                sb.Append("        registry.RegisterVariable<");
                sb.Append(model.TypeName);
                sb.Append(">(0x");
                sb.Append(model.OpCode.ToString("X2", CultureInfo.InvariantCulture));
                if (!string.IsNullOrWhiteSpace(model.Description))
                {
                    sb.Append(", \"");
                    sb.Append(EscapeStringLiteral(model.Description));
                    sb.Append('"');
                }
                sb.AppendLine(");");
            }
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static PacketModel? CreatePacketModel(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        if (context.Attributes.Length == 0)
        {
            return null;
        }

        var attribute = context.Attributes[0];

        if (attribute.ConstructorArguments.Length < 2)
        {
            return null;
        }

        var opCode = (byte)attribute.ConstructorArguments[0].Value!;
        var sizingRaw = attribute.ConstructorArguments[1];

        if (sizingRaw.Type?.ToDisplayString() != PacketSizingTypeName || sizingRaw.Value is null)
        {
            return null;
        }

        var sizingValue = Convert.ToInt32(sizingRaw.Value, CultureInfo.InvariantCulture);
        var isFixed = sizingValue == 0;
        var length = -1;
        string? description = null;

        foreach (var pair in attribute.NamedArguments)
        {
            if (pair.Key == "Length" && pair.Value.Value is int namedLength)
            {
                length = namedLength;
            }

            if (pair.Key == "Description" && pair.Value.Value is string namedDescription)
            {
                description = namedDescription;
            }
        }

        var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            ;

        if (fullTypeName.StartsWith("global::", StringComparison.Ordinal))
        {
            fullTypeName = fullTypeName.Substring("global::".Length);
        }

        return new(fullTypeName, opCode, isFixed, length, description);
    }

    private static string EscapeStringLiteral(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
    }
}
