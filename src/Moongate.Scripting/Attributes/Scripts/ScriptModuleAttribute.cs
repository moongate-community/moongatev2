namespace DarkLilly.Scripting.Lua.Attributes.Scripts;

/// <summary>
/// Attribute to mark a class as a script module that will be exposed to JavaScript.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]

/// <summary>
/// Attribute that marks a class as a script module exposed to scripting languages.
/// </summary>
/// <summary>
/// Initializes a new instance of the ScriptModuleAttribute class.
/// </summary>
/// <param name="name">The name under which the module will be accessible in JavaScript.</param>
/// <param name="helpText">The optional help text describing the module's purpose.</param>
public class ScriptModuleAttribute(string name, string? helpText = null) : Attribute
{
    /// <summary>Gets the name under which the module will be accessible in JavaScript.</summary>
    public string Name { get; } = name;

    /// <summary>Gets the optional help text describing the module's purpose.</summary>
    public string? HelpText { get; } = helpText;
}
