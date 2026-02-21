namespace Moongate.Abstractions.Interfaces;

/// <summary>
/// Defines an interface for building an Object Property List (Mega Cliloc) efficiently.
/// Implementations should minimize allocations by writing directly to network buffers.
/// </summary>
public interface IPropertyList
{
    /// <summary>
    /// Adds a cliloc without arguments.
    /// </summary>
    void Add(uint clilocId);

    /// <summary>
    /// Adds a cliloc with a text string argument.
    /// </summary>
    void Add(uint clilocId, string argument);

    /// <summary>
    /// Adds a plain text string to the property list.
    /// Uses a generic cliloc internally.
    /// </summary>
    void Add(string text);

    /// <summary>
    /// Adds a cliloc with an integer argument.
    /// </summary>
    void Add(uint clilocId, int value);

    /// <summary>
    /// Adds a cliloc with a formatted double/float/integer.
    /// </summary>
    void Add(uint clilocId, double value);
}
