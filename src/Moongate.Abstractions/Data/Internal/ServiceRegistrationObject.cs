namespace Moongate.Abstractions.Data.Internal;

/// <summary>
/// Represents a service registration entry used to keep track of
/// service contract, implementation type, and ordering priority.
/// </summary>
/// <param name="ServiceType">The registered service contract type.</param>
/// <param name="ImplementationType">The registered implementation type.</param>
/// <param name="Priority">The registration priority for ordered execution scenarios.</param>
public record ServiceRegistrationObject(Type ServiceType, Type ImplementationType, int Priority);
