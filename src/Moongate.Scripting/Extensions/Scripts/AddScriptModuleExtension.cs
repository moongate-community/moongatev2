using DryIoc;
using Moongate.Core.Extensions.Container;
using Moongate.Scripting.Data.Internal;
using MoonSharp.Interpreter;

namespace Moongate.Scripting.Extensions.Scripts;

/// <summary>
/// Extension methods for registering Lua script modules in the dependency injection container.
/// </summary>
public static class AddScriptModuleExtension
{
    /// <param name="container">The dependency injection container.</param>
    extension(IContainer container)
    {
        /// <summary>
        /// Registers a user data type with the container for Lua scripting.
        /// </summary>
        public IContainer RegisterLuaUserData(Type userDataType)
        {
            if (userDataType == null)
            {
                throw new ArgumentNullException(nameof(userDataType), "User data type cannot be null.");
            }

            container.AddToRegisterTypedList(new ScriptUserData { UserType = userDataType });

            return container;
        }

        /// <summary>
        /// Registers a user data type with the container for Lua scripting using generics.
        /// </summary>
        public IContainer RegisterLuaUserData<TUserData>()
        {
            UserData.RegisterType<TUserData>();

            return container.RegisterLuaUserData(typeof(TUserData));
        }

        /// <summary>
        /// Registers a Lua script module type with the container.
        /// </summary>
        /// <param name="scriptModule">The type of the script module to register.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when scriptModule is null.</exception>
        public IContainer RegisterScriptModule(Type scriptModule)
        {
            if (scriptModule == null)
            {
                throw new ArgumentNullException(nameof(scriptModule), "Script module type cannot be null.");
            }

            container.AddToRegisterTypedList(new ScriptModuleData(scriptModule));

            container.Register(scriptModule, Reuse.Singleton);

            return container;
        }

        /// <summary>
        /// Registers a Lua script module type with the container using a generic type parameter.
        /// </summary>
        /// <typeparam name="TScriptModule">The type of the script module to register.</typeparam>
        /// <returns>The container for method chaining.</returns>
        public IContainer RegisterScriptModule<TScriptModule>() where TScriptModule : class
            => container.RegisterScriptModule(typeof(TScriptModule));
    }
}
