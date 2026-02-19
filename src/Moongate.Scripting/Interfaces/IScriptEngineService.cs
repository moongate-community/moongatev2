using Moongate.Abstractions.Interfaces.Services.Base;
using Moongate.Scripting.Data.Scripts;

namespace Moongate.Scripting.Interfaces;

/// <summary>
/// Interface for the script engine service that manages JavaScript execution.
/// </summary>
public interface IScriptEngineService : IMoongateService
{
    /// <summary>
    /// Delegate for handling script file change events.
    /// </summary>
    /// <param name="filePath">The path to the changed file.</param>
    /// <returns>True if the file change was handled successfully, false otherwise.</returns>
    delegate bool LuaFileChangedHandler(string filePath);

    /// <summary>
    /// Event raised when a script file is modified.
    /// </summary>
    event LuaFileChangedHandler? FileChanged;

    /// <summary>
    /// Event raised when a script error occurs
    /// </summary>
    event EventHandler<ScriptErrorInfo>? OnScriptError;

    /// <summary>
    /// Adds a callback function that can be called from JavaScript.
    /// </summary>
    /// <param name="name">The name of the callback function in JavaScript.</param>
    /// <param name="callback">The C# action to execute when the callback is invoked.</param>
    void AddCallback(string name, Action<object[]> callback);

    /// <summary>
    /// Adds a constant value accessible from JavaScript.
    /// </summary>
    /// <param name="name">The name of the constant in JavaScript.</param>
    /// <param name="value">The value of the constant.</param>
    void AddConstant(string name, object value);

    /// <summary>
    /// Adds a script to be executed during engine initialization.
    /// </summary>
    /// <param name="script">The JavaScript code to execute on startup.</param>
    void AddInitScript(string script);

    /// <summary>
    /// Adds a manual module function that can be called from scripts.
    /// </summary>
    /// <param name="moduleName">The name of the module.</param>
    /// <param name="functionName">The name of the function.</param>
    /// <param name="callback">The callback to execute when the function is called.</param>
    void AddManualModuleFunction(string moduleName, string functionName, Action<object[]> callback);

    /// <summary>
    /// Adds a typed manual module function that can be called from scripts.
    /// </summary>
    /// <typeparam name="TInput">The input parameter type.</typeparam>
    /// <typeparam name="TOutput">The output return type.</typeparam>
    /// <param name="moduleName">The name of the module.</param>
    /// <param name="functionName">The name of the function.</param>
    /// <param name="callback">The callback function to execute.</param>
    void AddManualModuleFunction<TInput, TOutput>(
        string moduleName,
        string functionName,
        Func<TInput?, TOutput> callback
    );

    /// <summary>
    /// Adds a .NET type as a module accessible from JavaScript.
    /// </summary>
    /// <param name="type">The type to register as a script module.</param>
    void AddScriptModule(Type type);

    /// <summary>
    /// Executes a LUA function by name with the provided arguments.
    /// </summary>
    /// <param name="functionName"></param>
    /// <param name="args"></param>
    void CallFunction(string functionName, params object[] args);

    /// <summary>
    /// Clears the script cache
    /// </summary>
    void ClearScriptCache();

    /// <summary>
    /// Executes a previously registered callback function.
    /// </summary>
    /// <param name="name">The name of the callback to execute.</param>
    /// <param name="args">Arguments to pass to the callback.</param>
    void ExecuteCallback(string name, params object[] args);

    /// <summary>
    /// Notifies the script engine that the engine initialization is complete and ready.
    /// </summary>
    void ExecuteEngineReady();

    /// <summary>
    /// Executes a JavaScript function and returns the result.
    /// </summary>
    /// <param name="command">The JavaScript function call to execute.</param>
    /// <returns>A ScriptResult containing the execution outcome.</returns>
    ScriptResult ExecuteFunction(string command);

    /// <summary>
    /// Asynchronously executes a JavaScript function and returns the result.
    /// </summary>
    /// <param name="command">The JavaScript function call to execute.</param>
    /// <returns>A task containing a ScriptResult with the execution outcome.</returns>
    Task<ScriptResult> ExecuteFunctionAsync(string command);

    /// <summary>
    /// Executes a function defined in the bootstrap script.
    /// </summary>
    /// <param name="name"></param>
    void ExecuteFunctionFromBootstrap(string name);

    /// <summary>
    /// Executes a JavaScript script string.
    /// </summary>
    /// <param name="script">The JavaScript code to execute.</param>
    void ExecuteScript(string script);

    /// <summary>
    /// Executes a JavaScript file.
    /// </summary>
    /// <param name="scriptFile">The path to the JavaScript file to execute.</param>
    void ExecuteScriptFile(string scriptFile);

    /// <summary>
    /// Gets execution metrics for performance monitoring
    /// </summary>
    /// <returns>Metrics about script execution</returns>
    ScriptExecutionMetrics GetExecutionMetrics();

    /// <summary>
    /// Registers a global object/value accessible from scripts.
    /// </summary>
    /// <param name="name">The name of the global in scripts.</param>
    /// <param name="value">The object/value to register.</param>
    void RegisterGlobal(string name, object value);

    /// <summary>
    /// Registers a global function that can be called from scripts.
    /// </summary>
    /// <param name="name">The name of the global function in scripts.</param>
    /// <param name="func">The delegate to register as a global function.</param>
    void RegisterGlobalFunction(string name, Delegate func);

    /// <summary>
    /// Converts a .NET method name to a JavaScript-compatible function name.
    /// </summary>
    /// <param name="name">The .NET method name to convert.</param>
    /// <returns>The JavaScript-compatible function name.</returns>
    string ToScriptEngineFunctionName(string name);

    /// <summary>
    /// Unregisters a global function or value.
    /// </summary>
    /// <param name="name">The name of the global to unregister.</param>
    /// <returns>True if the global was found and removed, false otherwise.</returns>
    bool UnregisterGlobal(string name);
}
