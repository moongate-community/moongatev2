using System.Reflection;
using MoonSharp.Interpreter;

namespace DarkLilly.Scripting.Lua.Proxies;

/// <summary>
/// A proxy class that implements an interface by delegating method calls to a MoonSharp Table.
/// </summary>
/// <typeparam name="T">The interface type to implement.</typeparam>
public class LuaProxy<T> : DispatchProxy
{
    public Table Table { get; set; }

    /// <summary>
    /// Invokes the Lua function corresponding to the method name on the associated table.
    /// </summary>
    /// <param name="targetMethod">The method information for the invoked method.</param>
    /// <param name="args">The arguments passed to the method.</param>
    /// <returns>The result of the Lua function call, converted to the appropriate type.</returns>
    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        var fn = Table.Get(targetMethod.Name);

        if (fn.Type != DataType.Function)
        {
            throw new MissingMethodException(targetMethod.Name);
        }

        var dynArgs = args
                      .Select(a => DynValue.FromObject(null, a))
                      .ToArray();
        var result = fn.Function.Call(dynArgs);

        return result.ToObject(targetMethod.ReturnType);
    }
}
