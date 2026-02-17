using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Moongate.Scripting.Descriptors;

/// <summary>
/// Generic UserData descriptor that adds support for string concatenation and conversion.
/// Implements the __tostring metamethod to allow Lua to convert any userdata type to strings.
/// </summary>
public class GenericUserDataDescriptor : StandardUserDataDescriptor
{
    private readonly bool _isXnaType;

    /// <summary>
    /// Creates a new descriptor for a type with reflection access mode.
    /// Automatically detects if the type is an XNA Framework type.
    /// </summary>
    /// <param name="type">The type to describe (can be XNA or any other .NET type).</param>
    public GenericUserDataDescriptor(Type type)
        : base(type, InteropAccessMode.Reflection)
    {
        ArgumentNullException.ThrowIfNull(type);
        _isXnaType = type.Namespace?.StartsWith("Microsoft.Xna.Framework", StringComparison.Ordinal) == true;
    }

    /// <summary>
    /// Converts the object to its string representation.
    /// This is used by MoonSharp when the object is converted to a string in Lua (concatenation, tostring(), etc.).
    /// </summary>
    /// <param name="obj">The object to convert to string.</param>
    /// <returns>
    /// For XNA types: Uses ToString() for a readable representation.
    /// For other types: Uses ToString() or the type name if ToString() returns null.
    /// </returns>
    /// <example>
    /// In Lua, this allows:
    /// <code>
    /// local vec = Vector2(10, 20)
    /// print("Position: " .. vec)  -- ✅ Calls AsString(vec) instead of erroring
    /// local str = tostring(vec)    -- ✅ Works with tostring()
    /// </code>
    /// </example>
    public override string AsString(object obj)
    {
        if (obj == null)
        {
            return "null";
        }

        // Use the object's ToString() method
        var str = obj.ToString();

        return string.IsNullOrWhiteSpace(str)
                   ?

                   // Fallback: use the type name if ToString() returns empty/null
                   $"{Type.Name}({{}})"
                   : str;
    }
}
