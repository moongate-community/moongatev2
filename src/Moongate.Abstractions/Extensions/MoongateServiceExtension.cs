using DryIoc;
using Moongate.Abstractions.Data.Internal;
using Moongate.Core.Extensions.Container;

namespace Moongate.Abstractions.Extensions;

/// <summary>
/// Provides extension methods to register Moongate services in the DI container
/// and track their registration metadata with an optional execution priority.
/// </summary>
public static class MoongateServiceExtension
{
    extension(Container container)
    {
        /// <summary>
        /// Registers a service contract and implementation as a singleton and stores
        /// the registration metadata for ordered resolution scenarios.
        /// </summary>
        /// <typeparam name="TService">The service contract type.</typeparam>
        /// <typeparam name="TImplementation">The concrete implementation type.</typeparam>
        /// <param name="priority">The registration priority used for sorting service chains.</param>
        /// <returns>The same <see cref="Container"/> instance to allow fluent chaining.</returns>
        public Container RegisterMoongateService<TService, TImplementation>(int priority = 0)
            where TService : class
            where TImplementation : class, TService
        {
            container.Register<TService, TImplementation>(Reuse.Singleton);

            container.AddToRegisterTypedList(new ServiceRegistrationObject(typeof(TService), typeof(TImplementation), priority));

            return container;
        }

        /// <summary>
        /// Registers a self-bound service as a singleton and stores
        /// the registration metadata with the given priority.
        /// </summary>
        /// <typeparam name="TService">The service type to register as both contract and implementation.</typeparam>
        /// <param name="priority">The registration priority used for sorting service chains.</param>
        /// <returns>The same <see cref="Container"/> instance to allow fluent chaining.</returns>
        public Container RegisterMoongateService<TService>(int priority = 0)
            where TService : class
        {
            return container.RegisterMoongateService<TService, TService>(priority);
        }

        /// <summary>
        /// Registers a service contract and implementation type as a singleton and stores
        /// the registration metadata for ordered resolution scenarios.
        /// </summary>
        /// <param name="serviceType">The service contract type.</param>
        /// <param name="implementationType">The concrete implementation type.</param>
        /// <param name="priority">The registration priority used for sorting service chains.</param>
        /// <returns>The same <see cref="Container"/> instance to allow fluent chaining.</returns>
        public Container RegisterMoongateService(
            Type serviceType,
            Type implementationType,
            int priority = 0
        )
        {
            container.Register(serviceType, implementationType, Reuse.Singleton);

            container.AddToRegisterTypedList(new ServiceRegistrationObject(serviceType, implementationType, priority));

            return container;
        }

        /// <summary>
        /// Registers a generic service contract with a runtime implementation type as a singleton
        /// and stores the registration metadata with the given priority.
        /// </summary>
        /// <typeparam name="TService">The service contract type.</typeparam>
        /// <param name="implementationType">The concrete implementation type.</param>
        /// <param name="priority">The registration priority used for sorting service chains.</param>
        /// <returns>The same <see cref="Container"/> instance to allow fluent chaining.</returns>
        public Container RegisterMoongateService<TService>(
            Type implementationType,
            int priority = 0
        )
        {
            container.Register(typeof(TService), implementationType, Reuse.Singleton);

            container.AddToRegisterTypedList(new ServiceRegistrationObject(typeof(TService), implementationType, priority));

            return container;
        }
    }
}
