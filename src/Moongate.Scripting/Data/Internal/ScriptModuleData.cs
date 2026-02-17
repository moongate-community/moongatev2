namespace DarkLilly.Scripting.Lua.Data.Internal;

/// <summary>
/// Record containing data about a script module for internal processing.
/// </summary>
/// <summary>
/// Initializes a new instance of the ScriptModuleData record.
/// </summary>
/// <param name="ModuleType">The .NET type of the script module.</param>
public record ScriptModuleData(Type ModuleType);
