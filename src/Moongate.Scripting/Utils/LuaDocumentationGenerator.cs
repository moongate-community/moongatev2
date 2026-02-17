using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using DarkLilly.Core.Extensions.Strings;
using DarkLilly.Scripting.Lua.Attributes.Scripts;
using DarkLilly.Scripting.Lua.Data.Internal;

namespace DarkLilly.Scripting.Lua.Utils;

/// <summary>
/// Utility class for generating Lua meta files with EmmyLua/LuaLS annotations
/// Automatically creates meta.lua files with function signatures, types, and documentation
/// </summary>
[RequiresUnreferencedCode(
    "This class uses reflection to analyze types for Lua meta generation and requires full type metadata."
)]
public static class LuaDocumentationGenerator
{
    private static readonly HashSet<Type> _processedTypes = new();
    private static readonly StringBuilder _classesBuilder = new();
    private static readonly StringBuilder _constantsBuilder = new();
    private static readonly StringBuilder _enumsBuilder = new();
    private static readonly HashSet<Type> _classTypesToGenerate = new();
    private static readonly Dictionary<Type, bool> _recordTypeCache = new();
    private static readonly Lock _syncLock = new();

    private static Func<string, string> _nameResolver = name => name.ToSnakeCase();

    /// <summary>
    /// List of enums found during documentation generation
    /// </summary>
    public static List<Type> FoundEnums { get; } = new(16);

    /// <summary>
    /// Adds a class type to be generated in the documentation
    /// </summary>
    public static void AddClassToGenerate(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        _classTypesToGenerate.Add(type);
    }

    /// <summary>
    /// Clears all internal caches and state
    /// </summary>
    public static void ClearCaches()
    {
        lock (_syncLock)
        {
            _processedTypes.Clear();
            _recordTypeCache.Clear();
            _classTypesToGenerate.Clear();
            FoundEnums.Clear();
            _classesBuilder.Clear();
            _constantsBuilder.Clear();
            _enumsBuilder.Clear();
        }
    }

    [SuppressMessage("Trimming", "IL2075:Reflection", Justification = "Reflection is required for script module analysis"),
     SuppressMessage(
         "Trimming",
         "IL2072:Reflection",
         Justification = "Reflection is required for parameter and return type analysis"
     )]

    /// <summary>
    ///     Generates Lua documentation meta file with all module functions, classes, and constants
    /// </summary>
    public static string GenerateDocumentation(
        string appName,
        string appVersion,
        List<ScriptModuleData> scriptModules,
        Dictionary<string, object> constants,
        Dictionary<string, IReadOnlyCollection<string>> manualModules,
        Func<string, string>? nameResolver = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appName);
        ArgumentException.ThrowIfNullOrWhiteSpace(appVersion);
        ArgumentNullException.ThrowIfNull(scriptModules);
        ArgumentNullException.ThrowIfNull(constants);
        ArgumentNullException.ThrowIfNull(manualModules);

        lock (_syncLock)
        {
            if (nameResolver != null)
            {
                _nameResolver = nameResolver;
            }

            var sb = new StringBuilder();
            sb.AppendLine("---@meta");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine(CultureInfo.InvariantCulture, $"--- {appName} v{appVersion} Lua API");
            sb.AppendLine(CultureInfo.InvariantCulture, $"--- Auto-generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("---");
            sb.AppendLine();

            // Save data before clearing (they may have been added via AddClassToGenerate, AddCommonXnaTypesToGenerate, or FoundEnums)
            var typesToGenerate = new List<Type>(_classTypesToGenerate);
            var foundEnums = new List<Type>(FoundEnums);

            // Reset processed types and builders
            _processedTypes.Clear();
            _classesBuilder.Clear();
            _constantsBuilder.Clear();
            _enumsBuilder.Clear();
            _classTypesToGenerate.Clear();
            FoundEnums.Clear();

            // Restore types and enums to generate
            foreach (var type in typesToGenerate)
            {
                _classTypesToGenerate.Add(type);
            }

            foreach (var enumType in foundEnums)
            {
                FoundEnums.Add(enumType);
            }

            var distinctConstants = constants
                                    .GroupBy(kvp => kvp.Key)
                                    .ToDictionary(g => g.Key, g => g.First().Value);

            ProcessConstants(distinctConstants);
            sb.Append(_constantsBuilder);

            foreach (var module in scriptModules)
            {
                var scriptModuleAttribute = module.ModuleType.GetCustomAttribute<ScriptModuleAttribute>();

                if (scriptModuleAttribute is null)
                {
                    continue;
                }

                var moduleName = scriptModuleAttribute.Name;
                var moduleHelpText = scriptModuleAttribute.HelpText;

                sb.AppendLine("---");
                sb.AppendLine(CultureInfo.InvariantCulture, $"--- {module.ModuleType.Name} module");

                if (!string.IsNullOrWhiteSpace(moduleHelpText))
                {
                    sb.AppendLine("---");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"--- {moduleHelpText}");
                }

                sb.AppendLine("---");
                sb.AppendLine(CultureInfo.InvariantCulture, $"---@class {moduleName}");

                // Get all methods with ScriptFunction attribute
                var methods = module.ModuleType
                                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                    .Where(m => m.GetCustomAttribute<ScriptFunctionAttribute>() is not null)
                                    .ToList();

                foreach (var method in methods)
                {
                    var scriptFunctionAttr = method.GetCustomAttribute<ScriptFunctionAttribute>();

                    if (scriptFunctionAttr is null)
                    {
                        continue;
                    }

                    var functionName = string.IsNullOrWhiteSpace(scriptFunctionAttr.FunctionName)
                                           ? _nameResolver(method.Name)
                                           : scriptFunctionAttr.FunctionName;

                    sb.AppendLine(CultureInfo.InvariantCulture, $"{moduleName}.{functionName} = function() end");
                }

                sb.AppendLine(CultureInfo.InvariantCulture, $"{moduleName} = {{}}");
                sb.AppendLine();

                // Now generate detailed function documentation
                foreach (var method in methods)
                {
                    var scriptFunctionAttr = method.GetCustomAttribute<ScriptFunctionAttribute>();

                    if (scriptFunctionAttr is null)
                    {
                        continue;
                    }

                    var functionName = string.IsNullOrWhiteSpace(scriptFunctionAttr.FunctionName)
                                           ? _nameResolver(method.Name)
                                           : scriptFunctionAttr.FunctionName;
                    var description = scriptFunctionAttr.HelpText ?? "No description available";

                    sb.AppendLine("---");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"--- {description}");
                    sb.AppendLine("---");

                    // Add parameter documentation
                    var parameters = method.GetParameters();

                    foreach (var param in parameters)
                    {
                        var isParams = param.IsDefined(typeof(ParamArrayAttribute), false);
                        var paramType = isParams
                                            ? ConvertToLuaType(param.ParameterType.GetElementType()!)
                                            : ConvertToLuaType(param.ParameterType);
                        var paramName = isParams ? "..." : param.Name ?? $"param{Array.IndexOf(parameters, param)}";
                        var paramDescription = GetParameterDescription(param, paramType);
                        sb.AppendLine(
                            CultureInfo.InvariantCulture,
                            $"---@param {_nameResolver(paramName)} {paramType} {paramDescription}"
                        );
                    }

                    // Add return type documentation
                    if (method.ReturnType != typeof(void) && method.ReturnType != typeof(Task))
                    {
                        var returnType = ConvertToLuaType(method.ReturnType);
                        var returnDescription = GetReturnDescription(method.ReturnType, returnType);
                        sb.AppendLine(CultureInfo.InvariantCulture, $"---@return {returnType} {returnDescription}");
                    }

                    // Function signature
                    sb.Append(CultureInfo.InvariantCulture, $"function {moduleName}.{functionName}(");

                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var param = parameters[i];
                        var isParams = param.IsDefined(typeof(ParamArrayAttribute), false);
                        var paramName = isParams ? "..." : param.Name ?? $"param{i}";
                        sb.Append(_nameResolver(paramName));

                        if (i < parameters.Length - 1)
                        {
                            sb.Append(", ");
                        }
                    }

                    sb.AppendLine(") end");
                    sb.AppendLine();
                }
            }

            if (manualModules.Count > 0)
            {
                foreach (var manualModule in manualModules.OrderBy(kvp => kvp.Key, StringComparer.Ordinal))
                {
                    var moduleName = manualModule.Key;
                    var functions = manualModule.Value;

                    sb.AppendLine("---");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"--- {moduleName} module ");
                    sb.AppendLine("---");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"---@class {moduleName}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"{moduleName} = {{}}");
                    sb.AppendLine();

                    foreach (var function in functions.OrderBy(f => f, StringComparer.Ordinal))
                    {
                        sb.AppendLine("---");
                        sb.AppendLine("--- Dynamically registered function");
                        sb.AppendLine("---");
                        sb.AppendLine("---@param ... any");
                        sb.AppendLine("---@return any");
                        sb.AppendLine(CultureInfo.InvariantCulture, $"function {moduleName}.{function}(...) end");
                        sb.AppendLine();
                    }
                }
            }

            // Generate all classes that were collected during type conversion
            GenerateAllClasses();

            // Append enums and classes
            sb.Append(_enumsBuilder);
            sb.AppendLine();
            sb.Append(_classesBuilder);

            // Add global declarations for all registered types
            sb.AppendLine();
            sb.AppendLine("--- Global type constructors");

            foreach (var type in _classTypesToGenerate)
            {
                var typeName = type.Name;
                sb.AppendLine(CultureInfo.InvariantCulture, $"{typeName} = {{}}");
            }

            return sb.ToString();
        }
    }

    private static bool CanProcessType(Type type)
        => true;

    [SuppressMessage("Trimming", "IL2070:Reflection", Justification = "Reflection is required for Lua type conversion"),
     SuppressMessage("Trimming", "IL2072:Reflection", Justification = "Reflection is required for Lua type conversion"),
     SuppressMessage("Trimming", "IL2062:Reflection", Justification = "Reflection is required for Lua type conversion")]
    private static string ConvertToLuaType(
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods |
            DynamicallyAccessedMemberTypes.PublicProperties |
            DynamicallyAccessedMemberTypes.PublicConstructors
        )]
        Type type
    )
    {
        ArgumentNullException.ThrowIfNull(type);

        // Handle ref/out/in parameters (ByRef types)
        if (type.IsByRef)
        {
            var underlyingType = type.GetElementType();

            if (underlyingType is not null)
            {
                return ConvertToLuaType(underlyingType);
            }
        }

        // Handle void
        if (type == typeof(void))
        {
            return "nil";
        }

        // Handle string
        if (type == typeof(string))
        {
            return "string";
        }

        // Handle numbers
        if (type == typeof(int) ||
            type == typeof(long) ||
            type == typeof(float) ||
            type == typeof(double) ||
            type == typeof(decimal) ||
            type == typeof(short) ||
            type == typeof(ushort) ||
            type == typeof(uint) ||
            type == typeof(ulong) ||
            type == typeof(byte) ||
            type == typeof(sbyte))
        {
            return "number";
        }

        // Handle boolean
        if (type == typeof(bool))
        {
            return "boolean";
        }

        // Handle Guid
        if (type == typeof(Guid))
        {
            return "string";
        }

        // Handle DateTime types
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
        {
            return "number";
        }

        // Handle TimeSpan
        if (type == typeof(TimeSpan))
        {
            return "number";
        }

        // Handle object
        if (type == typeof(object))
        {
            return "any";
        }

        // Handle Task (async)
        if (type == typeof(Task))
        {
            return "nil";
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var taskResultType = type.GetGenericArguments()[0];

            return ConvertToLuaType(taskResultType);
        }

        // Handle Span and Memory types
        if (type.IsGenericType)
        {
            var typeDef = type.GetGenericTypeDefinition();

            if (typeDef == typeof(Span<>) ||
                typeDef == typeof(Memory<>) ||
                typeDef == typeof(ReadOnlySpan<>) ||
                typeDef == typeof(ReadOnlyMemory<>))
            {
                var elementType = type.GetGenericArguments()[0];

                return $"{ConvertToLuaType(elementType)}[]";
            }
        }

        // Handle arrays
        if (type.IsArray)
        {
            var elementType = type.GetElementType();

            if (elementType is null)
            {
                return "table";
            }

            return $"{ConvertToLuaType(elementType)}[]";
        }

        // Handle tuples
        if (type.IsGenericType && type.Name.StartsWith("ValueTuple", StringComparison.Ordinal))
        {
            var tupleArgs = type.GetGenericArguments();
            var typeList = string.Join(", ", tupleArgs.Select(ConvertToLuaType));

            return $"table<integer, {typeList}>";
        }

        // Handle nullable types
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(type);

            if (underlyingType is null)
            {
                return "any";
            }

            return $"{ConvertToLuaType(underlyingType)}?";
        }

        // Handle generic types
        if (type.IsGenericType)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            var genericArgs = type.GetGenericArguments();

            // Handle Dictionary
            if (genericTypeDefinition == typeof(Dictionary<,>))
            {
                var keyType = ConvertToLuaType(genericArgs[0]);
                var valueType = ConvertToLuaType(genericArgs[1]);

                return $"table<{keyType}, {valueType}>";
            }

            // Handle IReadOnlyDictionary
            if (genericTypeDefinition == typeof(IReadOnlyDictionary<,>))
            {
                var keyType = ConvertToLuaType(genericArgs[0]);
                var valueType = ConvertToLuaType(genericArgs[1]);

                return $"table<{keyType}, {valueType}>";
            }

            // Handle List, IEnumerable, ICollection, IList, IReadOnlyList, IReadOnlyCollection
            if (genericTypeDefinition == typeof(List<>) ||
                genericTypeDefinition == typeof(IEnumerable<>) ||
                genericTypeDefinition == typeof(ICollection<>) ||
                genericTypeDefinition == typeof(IList<>) ||
                genericTypeDefinition == typeof(IReadOnlyList<>) ||
                genericTypeDefinition == typeof(IReadOnlyCollection<>))
            {
                return $"{ConvertToLuaType(genericArgs[0])}[]";
            }

            // Handle Action delegates
            if (genericTypeDefinition == typeof(Action) ||
                genericTypeDefinition.Name.StartsWith("Action`", StringComparison.Ordinal))
            {
                return "fun()";
            }

            // Handle Func delegates
            if (genericTypeDefinition.Name.StartsWith("Func`", StringComparison.Ordinal))
            {
                var returnType = ConvertToLuaType(genericArgs[^1]);

                return $"fun():{returnType}";
            }
        }

        // Handle enums
        if (type.IsEnum)
        {
            GenerateEnumClass(type);

            return _nameResolver(type.Name);
        }

        // Handle MoonSharp Closure (represents Lua functions)
        if (type.Name == "Closure" && type.Namespace == "MoonSharp.Interpreter")
        {
            return "function";
        }

        // Handle record types
        if (IsRecordType(type))
        {
            var className = type.Name;

            if (_processedTypes.Contains(type))
            {
                return className;
            }

            _classTypesToGenerate.Add(type);

            return className;
        }

        // Handle other complex types
        if ((type.IsClass || type.IsValueType) &&
            !type.IsPrimitive &&
            type.Namespace is not null &&
            !type.Namespace.StartsWith("System", StringComparison.Ordinal))
        {
            var className = type.Name;

            if (_processedTypes.Contains(type))
            {
                return className;
            }

            _classTypesToGenerate.Add(type);

            return className;
        }

        // Handle delegates
        if (typeof(Delegate).IsAssignableFrom(type))
        {
            return "function";
        }

        return "any";
    }

    [SuppressMessage(
        "Trimming",
        "IL2072:Reflection",
        Justification = "Reflection is required for constant value formatting"
    )]
    private static string FormatConstantValue(object? value, Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (value is null)
        {
            return "nil";
        }

        if (type == typeof(string))
        {
            return $"\"{value}\"";
        }

        if (type == typeof(bool))
        {
            return value.ToString()!.ToLowerInvariant();
        }

        if (type.IsEnum)
        {
            return $"{_nameResolver(type.Name)}.{value}";
        }

        return value.ToString() ?? "nil";
    }

    /// <summary>
    /// Generate all classes after collecting them, with robust circular dependency handling
    /// </summary>
    private static void GenerateAllClasses()
    {
        var processedInIteration = new HashSet<Type>();
        var remainingTypes = new List<Type>(_classTypesToGenerate);
        var maxIterations = remainingTypes.Count + 5;
        var iterationCount = 0;

        while (remainingTypes.Count > 0 && iterationCount < maxIterations)
        {
            iterationCount++;
            processedInIteration.Clear();

            for (var i = remainingTypes.Count - 1; i >= 0; i--)
            {
                var type = remainingTypes[i];

                if (_processedTypes.Contains(type))
                {
                    remainingTypes.RemoveAt(i);

                    continue;
                }

                if (CanProcessType(type))
                {
                    GenerateClass(type);
                    processedInIteration.Add(type);
                    remainingTypes.RemoveAt(i);
                }
            }

            // If no progress was made, force process remaining types to avoid infinite loop
            if (processedInIteration.Count == 0 && remainingTypes.Count > 0)
            {
                foreach (var type in remainingTypes)
                {
                    if (!_processedTypes.Contains(type))
                    {
                        GenerateClass(type);
                    }
                }

                break;
            }
        }
    }

    /// <summary>
    /// Generate a single class with properties, constructors, and methods
    /// </summary>
    [SuppressMessage("Trimming", "IL2070:Reflection", Justification = "Reflection is required for class generation"),
     SuppressMessage("Trimming", "IL2072:Reflection", Justification = "Reflection is required for class generation")]
    private static void GenerateClass(
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicProperties |
            DynamicallyAccessedMemberTypes.PublicConstructors |
            DynamicallyAccessedMemberTypes.PublicMethods
        )]
        Type type
    )
    {
        ArgumentNullException.ThrowIfNull(type);

        if (!_processedTypes.Add(type))
        {
            return;
        }

        var className = type.Name;

        _classesBuilder.AppendLine();
        _classesBuilder.AppendLine("---");

        if (IsRecordType(type))
        {
            _classesBuilder.AppendLine(CultureInfo.InvariantCulture, $"--- Record type {type.FullName}");
        }
        else
        {
            _classesBuilder.AppendLine(CultureInfo.InvariantCulture, $"--- Class {type.FullName}");
        }

        _classesBuilder.AppendLine("---");
        _classesBuilder.AppendLine(CultureInfo.InvariantCulture, $"---@class {className}");

        // Generate properties with documentation (exclude indexers which have parameters)
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                             .ToList();

        foreach (var property in properties)
        {
            var propertyName = property.Name;

            if (string.IsNullOrEmpty(propertyName))
            {
                continue;
            }

            var propertyType = ConvertToLuaType(property.PropertyType);
            var xmlDocAttr = property.GetCustomAttribute<DescriptionAttribute>();
            var description = xmlDocAttr?.Description ?? "Property";

            // Use original property name for built-in types (XNA), apply resolver for custom types
            var displayName = type.Namespace?.StartsWith("Microsoft.Xna.Framework", StringComparison.Ordinal) == true
                                  ? propertyName
                                  : _nameResolver(propertyName);

            _classesBuilder.AppendLine(
                CultureInfo.InvariantCulture,
                $"---@field {displayName} {propertyType} # {description}"
            );
        }

        // Generate public constructors
        var constructors = type.GetConstructors(BindingFlags.Public)
                               .Where(c => c.GetParameters().Length > 0)
                               .ToList();

        if (constructors.Count > 0)
        {
            _classesBuilder.AppendLine("---");
            _classesBuilder.AppendLine("--- Constructors:");

            // Check if it's an XNA type to preserve original names
            var isXnaType = type.Namespace?.StartsWith("Microsoft.Xna.Framework", StringComparison.Ordinal) == true;

            foreach (var constructor in constructors)
            {
                _classesBuilder.Append("---@overload fun(");

                var parameters = constructor.GetParameters();

                for (var i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    var paramType = ConvertToLuaType(param.ParameterType);

                    // Use original parameter names for XNA types, apply resolver for custom types
                    var paramName = isXnaType
                                        ? param.Name ?? $"param{i}"
                                        : _nameResolver(param.Name ?? $"param{i}");
                    _classesBuilder.Append(CultureInfo.InvariantCulture, $"{paramName}: {paramType}");

                    if (i < parameters.Length - 1)
                    {
                        _classesBuilder.Append(", ");
                    }
                }

                _classesBuilder.AppendLine(CultureInfo.InvariantCulture, $"):void");
            }
        }

        // Generate public methods (excluding getters/setters from properties)
        var propertyMethods = new HashSet<string>();

        foreach (var prop in properties)
        {
            if (prop.GetMethod != null)
            {
                propertyMethods.Add(prop.GetMethod.Name);
            }

            if (prop.SetMethod != null)
            {
                propertyMethods.Add(prop.SetMethod.Name);
            }
        }

        // Methods to exclude (system methods that are not useful for Lua)
        var excludedMethods = new HashSet<string>
        {
            "GetHashCode",
            "Equals",
            "ToString",
            "GetType",
            "MemberwiseClone"
        };

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                          .Where(
                              m => !propertyMethods.Contains(m.Name) &&
                                   !m.IsSpecialName &&
                                   !excludedMethods.Contains(m.Name)
                          )
                          .ToList();

        if (methods.Count > 0)
        {
            _classesBuilder.AppendLine("---");
            _classesBuilder.AppendLine("--- Methods:");

            // Group methods by name to handle overloads
            var methodsByName = methods.GroupBy(m => m.Name);

            // Check if it's an XNA type to preserve original names
            var isXnaType = type.Namespace?.StartsWith("Microsoft.Xna.Framework", StringComparison.Ordinal) == true;

            foreach (var methodGroup in methodsByName)
            {
                var methodName = methodGroup.Key;

                foreach (var method in methodGroup)
                {
                    var returnType = method.ReturnType == typeof(void)
                                         ? "nil"
                                         : ConvertToLuaType(method.ReturnType);

                    var parameters = method.GetParameters();

                    _classesBuilder.Append("---@overload fun(");

                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var param = parameters[i];
                        var paramType = ConvertToLuaType(param.ParameterType);

                        // Use original parameter names for XNA types, apply resolver for custom types
                        var paramName = isXnaType
                                            ? param.Name ?? $"param{i}"
                                            : _nameResolver(param.Name ?? $"param{i}");
                        _classesBuilder.Append(CultureInfo.InvariantCulture, $"{paramName}: {paramType}");

                        if (i < parameters.Length - 1)
                        {
                            _classesBuilder.Append(", ");
                        }
                    }

                    _classesBuilder.AppendLine(CultureInfo.InvariantCulture, $"):{returnType}");
                }
            }
        }

        _classesBuilder.AppendLine();
    }

    [SuppressMessage(
        "Trimming",
        "IL2070:Reflection",
        Justification = "Reflection is required for enum class generation"
    )]
    private static void GenerateEnumClass(Type enumType)
    {
        ArgumentNullException.ThrowIfNull(enumType);

        if (!enumType.IsEnum)
        {
            throw new ArgumentException("Type must be an enum", nameof(enumType));
        }

        if (!_processedTypes.Add(enumType))
        {
            return;
        }

        FoundEnums.Add(enumType);

        _enumsBuilder.AppendLine();
        _enumsBuilder.AppendLine("---");
        _enumsBuilder.AppendLine(CultureInfo.InvariantCulture, $"--- Enum: {enumType.FullName}");
        _enumsBuilder.AppendLine("--- This enum is read-only and case-insensitive");
        _enumsBuilder.AppendLine("---");
        _enumsBuilder.AppendLine(CultureInfo.InvariantCulture, $"---@class {_nameResolver(enumType.Name)}");

        var enumValues = Enum.GetNames(enumType);
        var enumUnderlyingType = Enum.GetUnderlyingType(enumType);

        foreach (var value in enumValues)
        {
            try
            {
                var enumValue = Enum.Parse(enumType, value);
                var numericValue = Convert.ChangeType(enumValue, enumUnderlyingType, CultureInfo.InvariantCulture);
                _enumsBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"---@field public readonly {value} number # Enum value: {numericValue}"
                );
            }
            catch (Exception ex) when (ex is InvalidCastException or OverflowException)
            {
                _enumsBuilder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"---@field public readonly {value} number # Unable to determine value"
                );
            }
        }

        _enumsBuilder.AppendLine();
        var enumTypeName = _nameResolver(enumType.Name);
        _enumsBuilder.AppendLine(CultureInfo.InvariantCulture, $"--- Read-only enum table");
        _enumsBuilder.AppendLine(CultureInfo.InvariantCulture, $"{enumTypeName} = {{}}");
        _enumsBuilder.AppendLine();
    }

    /// <summary>
    /// Gets enhanced parameter description with type information
    /// </summary>
    private static string GetParameterDescription(ParameterInfo param, string luaType)
    {
        var isParams = param.IsDefined(typeof(ParamArrayAttribute), false);
        var baseName = param.Name ?? "parameter";

        string description;

        if (isParams)
        {
            description = "The arguments";
        }
        else
        {
            description = $"The {baseName.ToLowerInvariant()}";
        }

        if (luaType.Contains("number"))
        {
            description += isParams ? " values" : " value";
        }
        else if (luaType.Contains("string"))
        {
            description += isParams ? " texts" : " text";
        }
        else if (luaType.Contains("boolean"))
        {
            description += isParams ? " flags" : " flag";
        }
        else if (luaType.Contains("[]") || luaType.Contains("table"))
        {
            description += " table";
        }
        else
        {
            description += isParams ? $" of type {luaType}" : $" of type {luaType}";
        }

        if (param.IsOptional && !isParams)
        {
            description += " (optional)";
        }

        return description;
    }

    /// <summary>
    /// Gets enhanced return description with type information
    /// </summary>
    private static string GetReturnDescription(Type returnType, string luaType)
    {
        var description = "The ";

        if (luaType.Contains("number"))
        {
            description += "computed numeric value";
        }
        else if (luaType.Contains("string"))
        {
            description += "resulting text";
        }
        else if (luaType.Contains("boolean"))
        {
            description += "result of the operation";
        }
        else if (luaType.Contains("[]") || luaType.Contains("table"))
        {
            description += "collection of results";
        }
        else if (luaType.Contains("nil"))
        {
            description += "operation completes without returning a value";
        }
        else
        {
            description += $"result as {luaType}";
        }

        return description;
    }

    /// <summary>
    /// Check if a type is a C# record type
    /// </summary>
    [SuppressMessage("Trimming", "IL2070:Reflection", Justification = "Reflection is required for record type detection")]
    private static bool IsRecordType(
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.PublicMethods
        )]
        Type type
    )
    {
        ArgumentNullException.ThrowIfNull(type);

        if (_recordTypeCache.TryGetValue(type, out var isRecord))
        {
            return isRecord;
        }

        if (!type.IsClass)
        {
            _recordTypeCache[type] = false;

            return false;
        }

        var equalityContract = type.GetProperty(
            "EqualityContract",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (equalityContract is not null && equalityContract.PropertyType == typeof(Type))
        {
            _recordTypeCache[type] = true;

            return true;
        }

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var hasCompilerGeneratedToString = methods.Any(
            m =>
                m.Name == "ToString" &&
                m.GetParameters().Length == 0 &&
                m.GetCustomAttributes().Any(attr => attr.GetType().Name.Contains("CompilerGenerated"))
        );

        var result = hasCompilerGeneratedToString;
        _recordTypeCache[type] = result;

        return result;
    }

    [SuppressMessage("Trimming", "IL2072:Reflection", Justification = "Reflection is required for constant type analysis")]
    private static void ProcessConstants(Dictionary<string, object> constants)
    {
        ArgumentNullException.ThrowIfNull(constants);

        if (constants.Count == 0)
        {
            return;
        }

        _constantsBuilder.AppendLine("--- Global constants");
        _constantsBuilder.AppendLine();

        foreach (var constant in constants)
        {
            var constantName = constant.Key ?? "unnamed";
            var constantValue = constant.Value;
            var constantType = constantValue?.GetType() ?? typeof(object);

            var luaType = ConvertToLuaType(constantType);
            var formattedValue = FormatConstantValue(constantValue, constantType);

            _constantsBuilder.AppendLine("---");
            _constantsBuilder.AppendLine(CultureInfo.InvariantCulture, $"--- {constantName} constant");
            _constantsBuilder.AppendLine(CultureInfo.InvariantCulture, $"--- Value: {formattedValue}");
            _constantsBuilder.AppendLine("---");
            _constantsBuilder.AppendLine(CultureInfo.InvariantCulture, $"---@type {luaType}");
            _constantsBuilder.AppendLine(CultureInfo.InvariantCulture, $"{constantName} = {formattedValue}");
            _constantsBuilder.AppendLine();
        }

        _constantsBuilder.AppendLine();
    }
}
