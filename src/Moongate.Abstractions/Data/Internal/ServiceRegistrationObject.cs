namespace Moongate.Abstractions.Data.Internal;

public record ServiceRegistrationObject(Type ServiceType, Type ImplementationType, int Priority);
