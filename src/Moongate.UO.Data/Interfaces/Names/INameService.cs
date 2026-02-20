using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.UO.Data.Interfaces.Names;

/// <summary>
/// Stores typed name pools and generates random names for entities.
/// </summary>
public interface INameService
{
    /// <summary>
    /// Adds names to a specific logical name type.
    /// </summary>
    /// <param name="type">Name pool key.</param>
    /// <param name="names">Names to append.</param>
    void AddNames(string type, params string[] names);

    /// <summary>
    /// Generates a random name from the specified pool.
    /// </summary>
    /// <param name="type">Name pool key.</param>
    /// <returns>A generated name, or empty string when no names exist for the type.</returns>
    string GenerateName(string type);

    /// <summary>
    /// Generates a random name for a mobile template.
    /// </summary>
    /// <param name="mobileTemplate">Mobile template.</param>
    /// <returns>A generated name, or empty string when no suitable pool exists.</returns>
    string GenerateName(MobileTemplateDefinition mobileTemplate);
}
