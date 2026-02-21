using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DryIoc;
using Moongate.Core.Data.Directories;
using Moongate.Core.Extensions.Strings;
using Moongate.Core.Json;
using Moongate.Scripting.Attributes.Scripts;
using Moongate.Scripting.Context;
using Moongate.Scripting.Data.Config;
using Moongate.Scripting.Data.Internal;
using Moongate.Scripting.Data.Luarc;
using Moongate.Scripting.Data.Scripts;
using Moongate.Scripting.Interfaces;
using Moongate.Scripting.Loaders;
using Moongate.Scripting.Utils;
using MoonSharp.Interpreter;
using Serilog;
using SyntaxErrorException = System.Data.SyntaxErrorException;

#pragma warning disable IL2026 // RequiresUnreferencedCode - Lua scripting uses reflection for dynamic functionality
#pragma warning disable IL2072 // DynamicallyAccessedMemberTypes - Reflection access is necessary for scripting

namespace Moongate.Scripting.Services;

/// <summary>
/// Lua engine service that integrates MoonSharp with the SquidCraft game engine
/// Provides script execution, module loading, and Lua meta file generation
/// </summary>
public class LuaScriptEngineService : IScriptEngineService, IDisposable
{
    private static readonly string[] collection = ["log", "delay"];

    private readonly LuaEngineConfig _engineConfig;

    public event IScriptEngineService.LuaFileChangedHandler? FileChanged;

    private const string OnReadyFunctionName = "on_ready";

    private const string OnEngineRunFunctionName = "on_initialize";

    // Thread-safe collections
    private readonly ConcurrentDictionary<string, Action<object[]>> _callbacks = new();
    private readonly ConcurrentDictionary<string, object> _constants = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _manualModuleFunctions = new();

    private readonly DirectoriesConfig _directoriesConfig;
    private readonly List<string> _initScripts;
    private readonly ConcurrentDictionary<string, object> _loadedModules = new();
    private readonly ILogger _logger = Log.ForContext<LuaScriptEngineService>();

    // Cache compiled script chunks by content hash to avoid re-compiling identical scripts.
    private readonly ConcurrentDictionary<string, DynValue> _scriptCache = new();
    private readonly Lock _scriptExecutionSync = new();
    private readonly List<ScriptModuleData> _scriptModules;
    private readonly List<ScriptUserData> _loadedUserData;

    private readonly IContainer _serviceProvider;
    private int _cacheHits;
    private int _cacheMisses;

    private bool _disposed;
    private bool _isInitialized;
    private Func<string, string> _nameResolver;

    private FileSystemWatcher? _watcher;

    /// <summary>
    /// Initializes a new instance of the LuaScriptEngineService class.
    /// </summary>
    /// <param name="directoriesConfig">The directories configuration.</param>
    /// <param name="scriptModules">The list of script modules.</param>
    /// <param name="loadedUserData">The list of loaded user data.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="versionService">The version service.</param>
    public LuaScriptEngineService(
        DirectoriesConfig directoriesConfig,
        List<ScriptModuleData> scriptModules,
        IContainer serviceProvider,
        LuaEngineConfig engineConfig,
        List<ScriptUserData> loadedUserData = null
    )
    {
        JsonUtils.RegisterJsonContext(MoongateLuaScriptJsonContext.Default);

        ArgumentNullException.ThrowIfNull(directoriesConfig);
        ArgumentNullException.ThrowIfNull(scriptModules);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _scriptModules = scriptModules;
        _directoriesConfig = directoriesConfig;
        _serviceProvider = serviceProvider;
        _engineConfig = engineConfig;
        _loadedUserData = loadedUserData ?? new();
        _initScripts = ["bootstrap.lua", "init.lua", "main.lua"];

        CreateNameResolver();

        LuaScript = CreateOptimizedEngine();

        LoadToUserData();
    }

    /// <summary>
    /// Gets the MoonSharp script instance.
    /// </summary>
    public Script LuaScript { get; }

    /// <summary>
    /// Event raised when a script error occurs
    /// </summary>
    public event EventHandler<ScriptErrorInfo>? OnScriptError;

    /// <summary>
    /// Gets the script engine instance.
    /// </summary>
    public object Engine => LuaScript;

    /// <summary>
    /// Adds a callback function that can be called from Lua scripts.
    /// </summary>
    /// <param name="name">The name of the callback.</param>
    /// <param name="callback">The callback action.</param>
    public void AddCallback(string name, Action<object[]> callback)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(callback);

        var normalizedName = name.ToSnakeCaseUpper();
        _callbacks[normalizedName] = callback;

        _logger.Debug("Callback registered: {Name}", normalizedName);
    }

    /// <summary>
    /// Adds a constant value that can be accessed from Lua scripts.
    /// </summary>
    /// <param name="name">The name of the constant.</param>
    /// <param name="value">The value of the constant.</param>
    public void AddConstant(string name, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var normalizedName = name.ToSnakeCaseUpper();

        if (_constants.ContainsKey(normalizedName))
        {
            _logger.Warning("Constant {Name} already exists, overwriting", normalizedName);
        }

        _constants[normalizedName] = value;

        var valueToSet = value;

        if (value != null && !IsSimpleType(value.GetType()))
        {
            valueToSet = ObjectToTable(value);
        }

        LuaScript.Globals[normalizedName] = valueToSet;

        _logger.Debug("Constant added: {Name}", normalizedName);
    }

    /// <summary>
    /// Adds an initialization script.
    /// </summary>
    /// <param name="script">The script to add.</param>
    public void AddInitScript(string script)
    {
        if (string.IsNullOrWhiteSpace(script))
        {
            throw new ArgumentException("Script cannot be null or empty", nameof(script));
        }

        _initScripts.Add(script);
    }

    /// <summary>
    /// Adds a manual module function that can be called from Lua scripts with a callback.
    /// </summary>
    public void AddManualModuleFunction(string moduleName, string functionName, Action<object[]> callback)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);
        ArgumentException.ThrowIfNullOrWhiteSpace(functionName);
        ArgumentNullException.ThrowIfNull(callback);

        var (normalizedModule, normalizedFunction, moduleTable) = PrepareManualModule(moduleName, functionName);

        moduleTable[normalizedFunction] = DynValue.NewCallback(
            (_, args) =>
            {
                try
                {
                    var parameters = ConvertArgumentsToArray(args);
                    callback(parameters);

                    return DynValue.Nil;
                }
                catch (Exception ex)
                {
                    _logger.Error(
                        ex,
                        "Error executing manual module action {FunctionName} in {ModuleName}",
                        normalizedFunction,
                        normalizedModule
                    );

                    throw new ScriptRuntimeException(ex.Message);
                }
            }
        );

        RegisterManualModuleFunction(normalizedModule, normalizedFunction);
    }

    /// <summary>
    /// Adds a manual module function with typed input and output that can be called from Lua scripts.
    /// </summary>
    public void AddManualModuleFunction<TInput, TOutput>(
        string moduleName,
        string functionName,
        Func<TInput?, TOutput> callback
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);
        ArgumentException.ThrowIfNullOrWhiteSpace(functionName);
        ArgumentNullException.ThrowIfNull(callback);

        var (normalizedModule, normalizedFunction, moduleTable) = PrepareManualModule(moduleName, functionName);

        moduleTable[normalizedFunction] = DynValue.NewCallback(
            (_, args) =>
            {
                try
                {
                    var input = PrepareManualInput<TInput>(args);
                    var result = callback(input);

                    return ConvertToLua(result);
                }
                catch (Exception ex)
                {
                    _logger.Error(
                        ex,
                        "Error executing manual module function {FunctionName} in {ModuleName}",
                        normalizedFunction,
                        normalizedModule
                    );

                    throw new ScriptRuntimeException(ex.Message);
                }
            }
        );

        RegisterManualModuleFunction(normalizedModule, normalizedFunction);
    }

    /// <summary>
    /// Adds a script module to the engine.
    /// </summary>
    /// <param name="type">The type of the script module.</param>
    public void AddScriptModule(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        _scriptModules.Add(new(type));
    }

    public void CallFunction(string functionName, params object[] args)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(functionName);

        var luaFunction = LuaScript.Globals.Get(functionName);

        if (luaFunction.Type == DataType.Function)
        {
            try
            {
                var dynArgs = new DynValue[args.Length];

                for (var i = 0; i < args.Length; i++)
                {
                    dynArgs[i] = ConvertToLua(args[i]);
                }

                LuaScript.Call(luaFunction, dynArgs);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error calling Lua function {FunctionName}", functionName);

                throw;
            }
        }
        else
        {
            _logger.Warning("Lua function {FunctionName} not found or is not a function", functionName);
        }
    }

    /// <summary>
    /// Clears the script cache
    /// </summary>
    public void ClearScriptCache()
    {
        _scriptCache.Clear();
        _cacheHits = 0;
        _cacheMisses = 0;
        _logger.Information("Script cache cleared");
    }

    /// <summary>
    /// Disposes of the resources used by the LuaScriptEngineService.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            _loadedModules.Clear();
            _callbacks.Clear();
            _constants.Clear();

            GC.SuppressFinalize(this);

            _logger.Debug("Lua engine disposed successfully");
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Error during Lua engine disposal");
        }
        finally
        {
            _disposed = true;
        }
    }

    /// <summary>
    /// Executes a registered callback with the specified arguments.
    /// </summary>
    /// <param name="name">The name of the callback.</param>
    /// <param name="args">The arguments to pass to the callback.</param>
    public void ExecuteCallback(string name, params object[] args)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var normalizedName = name.ToSnakeCaseUpper();

        if (_callbacks.TryGetValue(normalizedName, out var callback))
        {
            try
            {
                _logger.Debug("Executing callback {Name}", normalizedName);
                callback(args);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error executing callback {Name}", normalizedName);

                throw;
            }
        }
        else
        {
            _logger.Warning("Callback {Name} not found", normalizedName);
        }
    }

    /// <summary>
    /// Executes the engine ready function from bootstrap scripts.
    /// </summary>
    public void ExecuteEngineReady()
    {
        ExecuteFunctionFromBootstrap(OnEngineRunFunctionName);
    }

    /// <summary>
    /// Executes a Lua function and returns the result.
    /// </summary>
    /// <param name="command">The function command to execute.</param>
    /// <returns>The result of the function execution.</returns>
    public ScriptResult ExecuteFunction(string command)
    {
        try
        {
            var result = LuaScript.DoString($"return {command}");

            return ScriptResultBuilder.CreateSuccess().WithData(result.ToObject()).Build();
        }
        catch (ScriptRuntimeException luaEx)
        {
            var errorInfo = CreateErrorInfo(luaEx, command);
            OnScriptError?.Invoke(this, errorInfo);

            _logger.Error(
                luaEx,
                "Lua error at line {Line}, column {Column}: {Message}",
                errorInfo.LineNumber,
                errorInfo.ColumnNumber,
                errorInfo.Message
            );

            return ScriptResultBuilder.CreateError()
                                      .WithMessage(
                                          $"{errorInfo.ErrorType}: {errorInfo.Message} at line {errorInfo.LineNumber}"
                                      )
                                      .Build();
        }
        catch (InterpreterException luaEx)
        {
            var errorInfo = CreateErrorInfo(luaEx, command);
            OnScriptError?.Invoke(this, errorInfo);

            _logger.Error(
                luaEx,
                "Lua {ErrorType} at line {Line}, column {Column}: {Message}",
                errorInfo.ErrorType,
                errorInfo.LineNumber,
                errorInfo.ColumnNumber,
                errorInfo.Message
            );

            return ScriptResultBuilder.CreateError()
                                      .WithMessage(
                                          $"{errorInfo.ErrorType}: {errorInfo.Message} at line {errorInfo.LineNumber}"
                                      )
                                      .Build();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to execute function: {Command}", command);

            return ScriptResultBuilder.CreateError().WithMessage(ex.Message).Build();
        }
    }

    /// <summary>
    /// Executes a Lua function asynchronously and returns the result.
    /// </summary>
    /// <param name="command">The function command to execute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<ScriptResult> ExecuteFunctionAsync(string command)
        => ExecuteFunction(command);

    public void ExecuteFunctionFromBootstrap(string name)
    {
        try
        {
            var onReadyFunc = LuaScript.Globals.Get(name);

            if (onReadyFunc.Type == DataType.Nil)
            {
                _logger.Warning("No {FuncName} function defined in scripts", name);

                return;
            }

            // Verify it's actually a function before calling
            if (onReadyFunc.Type != DataType.Function)
            {
                _logger.Error(
                    "'{FuncName}' is defined but is not a function, it's a {Type}. Skipping execution.",
                    name,
                    onReadyFunc.Type
                );

                return;
            }

            LuaScript.Call(onReadyFunc);
            _logger.Debug("Boot function executed successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error executing onReady function");

            throw;
        }
    }

    /// <summary>
    /// Executes a script string.
    /// </summary>
    /// <param name="script">The script to execute.</param>
    public void ExecuteScript(string script)
    {
        ExecuteScript(script, null);
    }

    /// <summary>
    /// Executes a script from a file.
    /// </summary>
    /// <param name="scriptFile">The path to the script file.</param>
    public void ExecuteScriptFile(string scriptFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptFile);

        if (!File.Exists(scriptFile))
        {
            throw new FileNotFoundException($"Script file not found: {scriptFile}", scriptFile);
        }

        try
        {
            var content = File.ReadAllText(scriptFile);
            _logger.Debug("Executing script file: {FileName}", Path.GetFileName(scriptFile));
            ExecuteScript(content, scriptFile);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to execute script file: {FileName}", Path.GetFileName(scriptFile));
        }
    }

    /// <summary>
    /// Executes a script file asynchronously.
    /// </summary>
    /// <param name="scriptFile">The path to the script file.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ExecuteScriptFileAsync(string scriptFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptFile);

        if (!File.Exists(scriptFile))
        {
            throw new FileNotFoundException($"Script file not found: {scriptFile}", scriptFile);
        }

        try
        {
            var content = await File.ReadAllTextAsync(scriptFile).ConfigureAwait(false);
            _logger.Debug("Executing script file asynchronously: {FileName}", Path.GetFileName(scriptFile));
            ExecuteScript(content, scriptFile);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to execute script file asynchronously: {FileName}", Path.GetFileName(scriptFile));

            throw;
        }
    }

    /// <summary>
    /// Gets execution metrics for performance monitoring
    /// </summary>
    public ScriptExecutionMetrics GetExecutionMetrics()
        => new()
        {
            CacheHits = _cacheHits,
            CacheMisses = _cacheMisses,
            TotalScriptsCached = _scriptCache.Count
        };

    /// <summary>
    /// Gets the statistics of the script engine.
    /// </summary>
    /// <returns>A tuple containing the module count, callback count, constant count, and initialization status.</returns>
    public (int ModuleCount, int CallbackCount, int ConstantCount, bool IsInitialized) GetStats()
        => (_loadedModules.Count, _callbacks.Count, _constants.Count, _isInitialized);

    /// <summary>
    /// Registers a global variable in the Lua environment.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="value">The value of the variable.</param>
    public void RegisterGlobal(string name, object value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);

        LuaScript.Globals[name] = value;
        _logger.Debug("Global registered: {Name} (Type: {Type})", name, value.GetType().Name);
    }

    /// <summary>
    /// Registers a global function in the Lua environment.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <param name="func">The delegate representing the function.</param>
    public void RegisterGlobalFunction(string name, Delegate func)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(func);

        LuaScript.Globals[name] = func;
        _logger.Debug("Global function registered: {Name}", name);
    }

    /// <summary>
    /// Registers a global type user data.
    /// </summary>
    /// <param name="type">The type to register.</param>
    public void RegisterGlobalTypeUserData(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        _logger.Debug("Global type user data registered: {TypeName}", type.Name);

        LuaScript.Globals[type.Name] = UserData.CreateStatic(type);
    }

    /// <summary>
    /// Registers a global type user data for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to register.</typeparam>
    public void RegisterGlobalTypeUserData<T>()
    {
        var type = typeof(T);
        _logger.Debug("Global type user data registered: {TypeName}", type.Name);

        LuaScript.Globals[type.Name] = UserData.CreateStatic(type);
    }

    /// <summary>
    /// Resets the script engine to its initial state.
    /// </summary>
    public void Reset()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _loadedModules.Clear();
        _callbacks.Clear();
        _constants.Clear();
        _isInitialized = false;

        _logger.Debug("Lua engine reset");
    }

    /// <summary>
    /// Stops the script engine asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ShutdownAsync()
        => Task.CompletedTask;

    /// <summary>
    /// Starts the script engine asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StartAsync()
    {
        if (_isInitialized)
        {
            _logger.Warning("Script engine is already initialized");

            return;
        }

        try
        {
            await RegisterScriptModulesAsync(CancellationToken.None);

            AddConstant("version", "0.0.1");
            AddConstant("engine", "Moongate");
            AddConstant("platform", Environment.OSVersion.Platform.ToString());

            _ = Task.Run(() => GenerateLuaMetaFileAsync(CancellationToken.None), CancellationToken.None);

            RegisterGlobalFunctions();

            ExecuteBootstrap();

            ExecuteBootFunction();
            _isInitialized = true;
            _logger.Information("Lua engine initialized successfully");

            if (_watcher == null)
            {
                _watcher = new(_engineConfig.ScriptsDirectory, "*.lua")
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true
                };

                _watcher.Changed += OnLuaFilesChanged;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize Lua engine");

            throw;
        }
    }

    public Task StopAsync()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;
        }

        _logger.Information("Lua engine stopped");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Converts a name to the script engine function name format.
    /// </summary>
    /// <param name="name">The name to convert.</param>
    /// <returns>The converted function name.</returns>
    public string ToScriptEngineFunctionName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return _nameResolver(name);
    }

    /// <summary>
    /// Unregisters a global variable from the Lua environment.
    /// </summary>
    /// <param name="name">The name of the variable to unregister.</param>
    /// <returns>True if the variable was unregistered, false otherwise.</returns>
    public bool UnregisterGlobal(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var existingValue = LuaScript.Globals.Get(name);

        if (existingValue.Type != DataType.Nil)
        {
            LuaScript.Globals[name] = DynValue.Nil;
            _logger.Debug("Global unregistered: {Name}", name);

            return true;
        }

        _logger.Warning("Attempted to unregister non-existent global: {Name}", name);

        return false;
    }

    private static object?[] ConvertArgumentsToArray(CallbackArguments args)
    {
        if (args.Count == 0)
        {
            return Array.Empty<object?>();
        }

        var converted = new object?[args.Count];

        for (var i = 0; i < args.Count; i++)
        {
            converted[i] = args[i].ToObject();
        }

        return converted;
    }

    private static object? ConvertFromLua(DynValue dynValue, Type targetType)
    {
        return dynValue.Type switch
        {
            DataType.Nil     => null,
            DataType.Boolean => dynValue.Boolean,
            DataType.Number  => Convert.ChangeType(dynValue.Number, targetType, CultureInfo.InvariantCulture),
            DataType.String  => dynValue.String,
            DataType.Table   => dynValue.ToObject(),
            _                => dynValue.ToObject()
        };
    }

    private DynValue ConvertToLua(object? value)
        => value == null ? DynValue.Nil : DynValue.FromObject(LuaScript, value);

    /// <summary>
    /// Creates a factory function that dynamically invokes the correct constructor.
    /// Uses reflection to find the constructor matching the number of arguments passed from Lua.
    /// </summary>
    private Func<object?, object?, object?, object?, object?> CreateConstructorWrapper(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type
    )
    {
        // Cache constructors by parameter count for performance
        var constructorsByParamCount = new Dictionary<int, ConstructorInfo>();
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        foreach (var ctor in constructors)
        {
            var paramCount = ctor.GetParameters().Length;
            constructorsByParamCount.TryAdd(paramCount, ctor);
        }

        return (arg1, arg2, arg3, arg4) =>
               {
                   // Collect arguments
                   var rawArgs = new List<object?>();

                   if (arg1 != null)
                   {
                       rawArgs.Add(arg1);
                   }

                   if (arg2 != null)
                   {
                       rawArgs.Add(arg2);
                   }

                   if (arg3 != null)
                   {
                       rawArgs.Add(arg3);
                   }

                   if (arg4 != null)
                   {
                       rawArgs.Add(arg4);
                   }

                   var argCount = rawArgs.Count;

                   // Find constructor with matching parameter count
                   if (constructorsByParamCount.TryGetValue(argCount, out var ctor))
                   {
                       try
                       {
                           // Convert arguments to match constructor parameter types
                           var parameters = ctor.GetParameters();
                           var convertedArgs = new object?[argCount];

                           for (var i = 0; i < argCount; i++)
                           {
                               var paramType = parameters[i].ParameterType;
                               var argValue = rawArgs[i];

                               // Convert argument to the expected parameter type
                               if (argValue == null)
                               {
                                   convertedArgs[i] = null;
                               }
                               else if (paramType.IsInstanceOfType(argValue))
                               {
                                   // No conversion needed
                                   convertedArgs[i] = argValue;
                               }
                               else
                               {
                                   // Convert using Convert.ChangeType (handles double -> float, etc.)
                                   try
                                   {
                                       convertedArgs[i] = Convert.ChangeType(
                                           argValue,
                                           paramType,
                                           CultureInfo.InvariantCulture
                                       );
                                   }
                                   catch
                                   {
                                       convertedArgs[i] = argValue; // Fallback to original value
                                   }
                               }
                           }

                           // Create instance with converted arguments
                           return Activator.CreateInstance(type, convertedArgs);
                       }
                       catch (Exception ex)
                       {
                           throw new ScriptRuntimeException(
                               $"Constructor of {type.Name} with {argCount} arguments failed: {ex.Message}",
                               ex
                           );
                       }
                   }

                   // No matching constructor found
                   var availableCtors = string.Join(", ", constructorsByParamCount.Keys.OrderBy(k => k));

                   throw new ScriptRuntimeException(
                       $"No constructor found for {type.Name} with {argCount} arguments. Available: {availableCtors}"
                   );
               };
    }

    /// <summary>
    /// Creates detailed error information from a Lua exception
    /// </summary>
    private static ScriptErrorInfo CreateErrorInfo(ScriptRuntimeException luaEx, string sourceCode, string? fileName = null)
    {
        var errorInfo = new ScriptErrorInfo
        {
            Message = luaEx.DecoratedMessage ?? luaEx.Message,
            StackTrace = luaEx.StackTrace,
            LineNumber = 0,
            ColumnNumber = 0,
            ErrorType = "LuaError",
            SourceCode = sourceCode,
            FileName = fileName ?? "script.lua"
        };

        return errorInfo;
    }

    /// <summary>
    /// Creates detailed error information from a Lua interpreter exception (syntax errors, etc.)
    /// </summary>
    private static ScriptErrorInfo CreateErrorInfo(InterpreterException luaEx, string sourceCode, string? fileName = null)
    {
        // Extract line and column info from the exception message if available
        // SyntaxErrorException typically has format like "chunk_1:(1,5-10): unexpected symbol near '?'"
        int? lineNumber = null;
        int? columnNumber = null;
        var errorType = "LuaError";

        if (luaEx is SyntaxErrorException)
        {
            errorType = "SyntaxError";
        }

        // Try to extract line and column from the message
        var message = luaEx.Message;

        if (message.Contains('('))
        {
            var match = Regex.Match(message, @"\((\d+),(\d+)");

            if (match.Success)
            {
                lineNumber = int.Parse(match.Groups[1].Value, CultureInfo.CurrentCulture);
                columnNumber = int.Parse(match.Groups[2].Value, CultureInfo.CurrentCulture);
            }
        }

        var errorInfo = new ScriptErrorInfo
        {
            Message = luaEx.DecoratedMessage ?? luaEx.Message,
            StackTrace = luaEx.StackTrace,
            LineNumber = lineNumber,
            ColumnNumber = columnNumber,
            ErrorType = errorType,
            SourceCode = sourceCode,
            FileName = fileName ?? "script.lua"
        };

        return errorInfo;
    }

    [UnconditionalSuppressMessage(
        "Aot",
        "IL3050",
        Justification = "Lua params-array conversion requires runtime element type resolution by reflection."
    )]
    private DynValue CreateMethodClosure(object instance, MethodInfo method)
    {
        return DynValue.NewCallback(
            (context, args) =>
            {
                try
                {
                    var parameters = method.GetParameters();

                    // Check if the last parameter is a params array
                    var hasParamsArray = parameters.Length > 0 &&
                                         parameters[^1].IsDefined(typeof(ParamArrayAttribute), false);

                    object?[] convertedArgs;

                    if (hasParamsArray)
                    {
                        var regularParamsCount = parameters.Length - 1;
                        convertedArgs = new object?[parameters.Length];

                        // Convert regular parameters
                        for (var i = 0; i < regularParamsCount && i < args.Count; i++)
                        {
                            convertedArgs[i] = ConvertFromLua(args[i], parameters[i].ParameterType);
                        }

                        // Collect remaining arguments into params array
                        var paramsArrayType = parameters[^1].ParameterType.GetElementType()!;
                        var paramsCount = Math.Max(0, args.Count - regularParamsCount);
                        var paramsArray = Array.CreateInstance(paramsArrayType, paramsCount);

                        for (var i = 0; i < paramsCount; i++)
                        {
                            var argIndex = regularParamsCount + i;
                            paramsArray.SetValue(ConvertFromLua(args[argIndex], paramsArrayType), i);
                        }

                        convertedArgs[^1] = paramsArray;
                    }
                    else
                    {
                        // Normal parameter handling
                        convertedArgs = new object?[parameters.Length];

                        for (var i = 0; i < parameters.Length && i < args.Count; i++)
                        {
                            convertedArgs[i] = ConvertFromLua(args[i], parameters[i].ParameterType);
                        }
                    }

                    var result = method.Invoke(instance, convertedArgs);

                    return method.ReturnType == typeof(void) ? DynValue.Nil : ConvertToLua(result);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error calling method {MethodName}", method.Name);

                    throw new ScriptRuntimeException(ex.Message);
                }
            }
        );
    }

    private Table CreateModuleTable(
        object instance,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
        Type moduleType
    )
    {
        var moduleTable = new Table(LuaScript);

        var methods = moduleType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                .Where(m => m.GetCustomAttribute<ScriptFunctionAttribute>() is not null);

        foreach (var method in methods)
        {
            var scriptFunctionAttr = method.GetCustomAttribute<ScriptFunctionAttribute>();

            if (scriptFunctionAttr is null)
            {
                continue;
            }

            var functionName = string.IsNullOrWhiteSpace(scriptFunctionAttr.FunctionName)
                                   ? _nameResolver(method.Name)
                                   : scriptFunctionAttr.FunctionName;

            // Create a closure that captures the instance and method
            var closure = CreateMethodClosure(instance, method);
            moduleTable[functionName] = closure;
        }

        return moduleTable;
    }

    private void CreateNameResolver()
    {
        _nameResolver = name => name.ToSnakeCase();

        // _nameResolver = _scriptEngineConfig.ScriptNameConversion switch
        // {
        //     ScriptNameConversion.CamelCase  => name => name.ToCamelCase(),
        //     ScriptNameConversion.PascalCase => name => name.ToPascalCase(),
        //     ScriptNameConversion.SnakeCase  => name => name.ToSnakeCase(),
        //     _                               => _nameResolver
        // };
    }

    private Script CreateOptimizedEngine()
    {
        var script = new Script
        {
            Options =
            {
                // Configure MoonSharp options
                DebugPrint = s => _logger.Debug("[Lua] {Message}", s),
                ScriptLoader = new LuaScriptLoader(_engineConfig.ScriptsDirectory)
            }
        };

        _logger.Debug("Lua script loader configured for require() functionality");

        return script;
    }

    private void ExecuteBootFunction()
    {
        ExecuteFunctionFromBootstrap(OnReadyFunctionName);
    }

    private void ExecuteBootstrap()
    {
        foreach (var file in _initScripts.Select(s => Path.Combine(_engineConfig.ScriptsDirectory, s)))
        {
            if (File.Exists(file))
            {
                var fileName = Path.GetFileName(file);
                _logger.Information("Executing {FileName} script", fileName);
                ExecuteScriptFile(file);
            }
        }
    }

    /// <summary>
    /// Executes a script string with an optional file name for error reporting.
    /// </summary>
    /// <param name="script">The script to execute.</param>
    /// <param name="fileName">Optional file name for error reporting.</param>
    private void ExecuteScript(string script, string? fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(script);

        var stopwatch = Stopwatch.GetTimestamp();

        try
        {
            var scriptHash = GetScriptHash(script);
            DynValue compiledScriptChunk;

            if (_scriptCache.TryGetValue(scriptHash, out compiledScriptChunk))
            {
                Interlocked.Increment(ref _cacheHits);
                _logger.Debug("Script found in cache");
            }
            else
            {
                Interlocked.Increment(ref _cacheMisses);

                // Compile first: invalid scripts must not pollute cache entries.
                var compiled = LuaScript.LoadString(script, null, fileName ?? "runtime_chunk");

                if (!_scriptCache.TryAdd(scriptHash, compiled))
                {
                    Interlocked.Increment(ref _cacheHits);
                    _scriptCache.TryGetValue(scriptHash, out compiledScriptChunk);
                }
                else
                {
                    compiledScriptChunk = compiled;
                }
            }

            lock (_scriptExecutionSync)
            {
                LuaScript.Call(compiledScriptChunk);
            }
            var elapsedMs = Stopwatch.GetElapsedTime(stopwatch);
            _logger.Debug("Script executed successfully in {ElapsedMs}ms", elapsedMs);
        }
        catch (ScriptRuntimeException luaEx)
        {
            var errorInfo = CreateErrorInfo(luaEx, script, fileName);
            OnScriptError?.Invoke(this, errorInfo);

            _logger.Error(
                luaEx,
                "Lua error at line {Line}, column {Column}: {Message}",
                errorInfo.LineNumber,
                errorInfo.ColumnNumber,
                errorInfo.Message
            );

            throw;
        }
        catch (InterpreterException luaEx)
        {
            var errorInfo = CreateErrorInfo(luaEx, script, fileName);
            OnScriptError?.Invoke(this, errorInfo);

            _logger.Error(
                luaEx,
                "Lua {ErrorType} at line {Line}, column {Column}: {Message}",
                errorInfo.ErrorType,
                errorInfo.LineNumber,
                errorInfo.ColumnNumber,
                errorInfo.Message
            );

            throw;
        }
        catch (Exception e)
        {
            var elapsedMs = Stopwatch.GetElapsedTime(stopwatch);
            _logger.Error(
                e,
                "Error executing script: {ScriptPreview}",
                script.Length > 100 ? script[..100] + "..." : script
            );

            throw;
        }
    }

    [RequiresUnreferencedCode(
        "Lua meta generation relies on reflection-heavy LuaDocumentationGenerator which is not trim-safe."
    )]
    private async Task GenerateLuaMetaFileAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.Debug("Generating Lua meta files");

            var definitionDirectory = _engineConfig.LuarcDirectory;

            if (!Directory.Exists(definitionDirectory))
            {
                Directory.CreateDirectory(definitionDirectory);
            }

            foreach (var userData in _loadedUserData)
            {
                // check if is enum

                if (userData.UserType.IsEnum)
                {
                    LuaDocumentationGenerator.FoundEnums.Add(userData.UserType);

                    continue;
                }

                LuaDocumentationGenerator.AddClassToGenerate(userData.UserType);
            }

            AddConstant("engine_version", _engineConfig.EngineVersion);

            // Generate meta.lua
            var manualModulesSnapshot = _manualModuleFunctions.ToDictionary(
                kvp => kvp.Key,
                IReadOnlyCollection<string> (kvp) => kvp.Value.Keys.ToArray()
            );

            var documentation = LuaDocumentationGenerator.GenerateDocumentation(
                "DarkLilly",
                _engineConfig.EngineVersion,
                _scriptModules,
                new(_constants),
                manualModulesSnapshot,
                _nameResolver
            );

            var metaLuaPath = Path.Combine(definitionDirectory, "definitions.lua");
            await File.WriteAllTextAsync(metaLuaPath, documentation, cancellationToken);
            _logger.Debug("Lua meta file generated at {Path}", metaLuaPath);

            // Generate .luarc.json
            var luarcJson = GenerateLuarcJson();
            var luarcPath = Path.Combine(_engineConfig.LuarcDirectory, ".luarc.json");
            await File.WriteAllTextAsync(luarcPath, luarcJson, cancellationToken);
            _logger.Debug("Lua configuration file generated at {Path}", luarcPath);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to generate Lua meta files");
        }
    }

    private string GenerateLuarcJson()
    {
        var globalsList = _constants.Keys.ToList();
        globalsList.AddRange(collection);

        // Add registered user data types (Vector3, Vector2, Quaternion, etc.)
        foreach (var userData in _loadedUserData)
        {
            globalsList.Add(userData.UserType.Name);
        }

        var luarcConfig = new LuarcConfig
        {
            Runtime = new()
            {
                Path =
                [
                    "?.lua",
                    "?/init.lua",
                    "modules/?.lua",
                    "modules/?/init.lua"
                ]
            },
            Workspace = new()
            {
                Library = [_engineConfig.ScriptsDirectory]
            },
            Diagnostics = new()
            {
                Globals = [..globalsList]
            }
        };

        return JsonSerializer.Serialize(luarcConfig, MoongateLuaScriptJsonContext.Default.LuarcConfig);
    }

    /// <summary>
    /// Generates a hash for script caching
    /// </summary>
    private static string GetScriptHash(string script)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(script));

        return Convert.ToBase64String(hashBytes);
    }

    private static bool IsSimpleType(Type type)
        => type.IsPrimitive || type == typeof(string) || type.IsEnum;

    private void LoadToUserData()
    {
        if (_loadedUserData == null)
        {
            return;
        }

        foreach (var scriptUserData in _loadedUserData)
        {
            // Register the type to allow MoonSharp to access its members and methods
            UserData.RegisterType(scriptUserData.UserType);

            // Check if type has public constructors (instantiable)
#pragma warning disable IL2075 // Suppress AOT warning for script proxy
            var publicConstructors = scriptUserData.UserType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
#pragma warning restore IL2075

            if (publicConstructors.Length > 0)
            {
                // Instantiable type - use constructor wrapper for easier instance creation
                var constructorWrapper = CreateConstructorWrapper(scriptUserData.UserType);
                LuaScript.Globals[scriptUserData.UserType.Name] = constructorWrapper;
            }
            else
            {
                // Static class or no public constructors - expose the type itself for static method access
                LuaScript.Globals[scriptUserData.UserType.Name] = scriptUserData.UserType;
            }

            _logger.Debug("User data type registered: {TypeName}", scriptUserData.UserType.Name);

            LuaDocumentationGenerator.AddClassToGenerate(scriptUserData.UserType);
        }
    }

    private Table ObjectToTable(object obj)
    {
        var table = new Table(LuaScript);
#pragma warning disable IL2075 // Suppress AOT warning for script proxy
        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
#pragma warning restore IL2075

        foreach (var prop in properties)
        {
            var value = prop.GetValue(obj);
            table[prop.Name] = value;
        }

        return table;
    }

    private void OnLuaFilesChanged(object sender, FileSystemEventArgs e)
    {
        if (_initScripts.Contains(e.Name))
        {
            _logger.Information("Lua script file changed: {FileName}. Clearing script cache.", e.Name);

            if (FileChanged != null)
            {
                ClearScriptCache();

                if (FileChanged(e.FullPath))
                {
                    _logger.Information("File change handled successfully: {FileName}", e.Name);

                    ExecuteBootstrap();
                    ExecuteBootFunction();
                }
            }
        }
    }

    private TInput? PrepareManualInput<TInput>(CallbackArguments args)
    {
        if (typeof(TInput) == typeof(object[]))
        {
            return (TInput?)(object?)ConvertArgumentsToArray(args);
        }

        if (args.Count == 0)
        {
            return default;
        }

        var firstArg = args[0];
        var converted = ConvertFromLua(firstArg, typeof(TInput));

        return converted is null ? default : (TInput?)converted;
    }

    private (string ModuleName, string FunctionName, Table ModuleTable) PrepareManualModule(
        string moduleName,
        string functionName
    )
    {
        var normalizedModuleName = _nameResolver(moduleName);
        var normalizedFunctionName = _nameResolver(functionName);

        var existing = LuaScript.Globals.Get(normalizedModuleName);
        Table moduleTable;

        if (existing.Type == DataType.Table)
        {
            moduleTable = existing.Table;
        }
        else
        {
            moduleTable = new(LuaScript);
            LuaScript.Globals[normalizedModuleName] = moduleTable;
        }

        _loadedModules.TryAdd(normalizedModuleName, moduleTable);

        return (normalizedModuleName, normalizedFunctionName, moduleTable);
    }

    [RequiresUnreferencedCode("Enum registration uses reflection to access enum metadata.")]
    private void RegisterEnum(Type enumType)
    {
        ArgumentNullException.ThrowIfNull(enumType);

        if (!enumType.IsEnum)
        {
            _logger.Warning("Type {TypeName} is not an enum, skipping registration", enumType.Name);

            return;
        }

        var enumName = _nameResolver(enumType.Name);
        var enumTable = new Table(LuaScript);
        var enumValuesByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Populate enum values
        var names = Enum.GetNames(enumType);
        var underlyingValues = Enum.GetValuesAsUnderlyingType(enumType);

        for (var i = 0; i < names.Length; i++)
        {
            var name = names[i];
            var rawValue = underlyingValues.GetValue(i);

            if (rawValue is null)
            {
                continue;
            }

            var coercedValue = Convert.ToInt32(rawValue, CultureInfo.InvariantCulture);
            enumTable[name] = coercedValue;
            enumValuesByName[name] = coercedValue;
        }

        // Create metatable for read-only and case-insensitive access
        var metatable = new Table(LuaScript);

        // __index: allows case-insensitive access
        metatable["__index"] = DynValue.NewCallback(
            (ctx, args) =>
            {
                var key = args[1].String;

                if (string.IsNullOrEmpty(key))
                {
                    return DynValue.Nil;
                }

                // Try exact match first
                var value = enumTable.Get(key);

                if (value.Type != DataType.Nil)
                {
                    return value;
                }

                // Try case-insensitive match
                if (enumValuesByName.TryGetValue(key, out var intValue))
                {
                    return DynValue.NewNumber(intValue);
                }

                _logger.Warning(
                    "Attempt to access undefined enum value {EnumName}.{ValueName}",
                    enumName,
                    key
                );

                return DynValue.Nil;
            }
        );

        // __newindex: prevents modifications (read-only)
        metatable["__newindex"] = DynValue.NewCallback(
            (ctx, args) =>
            {
                var key = args[1].String;

                throw new ScriptRuntimeException($"Cannot modify enum {enumName}.{key}: enums are read-only");
            }
        );

        // __tostring: pretty print
        metatable["__tostring"] = DynValue.NewCallback(
            (ctx, args) =>
            {
                return DynValue.NewString($"enum<{enumName}>");
            }
        );

        // Set the enum table first
        var enumTableDynValue = DynValue.NewTable(enumTable);

        // Try to apply metatable (may not work perfectly in all MoonSharp versions)
        try
        {
            // Create a reference for the metatable
            var metatableValue = DynValue.NewTable(metatable);
            enumTable.MetaTable = metatable;
        }
        catch
        {
            _logger.Warning("Could not apply metatable to enum {EnumName}, using fallback", enumName);
        }

        // Register the enum table in globals
        LuaScript.Globals[enumName] = enumTableDynValue;

        _logger.Debug(
            "Registered enum {EnumName} with {ValueCount} values (read-only, case-insensitive)",
            enumName,
            enumValuesByName.Count
        );
    }

    [RequiresUnreferencedCode("Enum metadata is discovered dynamically when building Lua documentation.")]
    private void RegisterEnums()
    {
        var enumsFound = LuaDocumentationGenerator.FoundEnums;

        foreach (var enumType in enumsFound)
        {
            RegisterEnum(enumType);
        }
    }

    private void RegisterGlobalFunctions()
    {
        LuaScript.Globals["delay"] = (Func<int, Task>)(async milliseconds =>
                                                       {
                                                           await Task.Delay(Math.Min(milliseconds, 5000));
                                                       });

        var existingLog = LuaScript.Globals.Get("log");

        if (existingLog.Type == DataType.Nil)
        {
            LuaScript.Globals["log"] = (Action<object>)(message => { _logger.Information("Lua: {Message}", message); });
        }
        else
        {
            // Keep script module "log" intact (e.g. log.info/log.error) and provide a fallback function alias.
            LuaScript.Globals["log_message"] =
                (Action<object>)(message => { _logger.Information("Lua: {Message}", message); });
        }

        LuaScript.Globals["toString"] = (Func<object, string>)(obj => obj?.ToString() ?? "nil");
    }

    private void RegisterManualModuleFunction(string moduleName, string functionName)
    {
        var functions = _manualModuleFunctions.GetOrAdd(moduleName, _ => new());
        functions.TryAdd(functionName, 0);
    }

    private async Task RegisterScriptModulesAsync(CancellationToken cancellationToken)
    {
        foreach (var module in _scriptModules)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var scriptModuleAttribute = module.ModuleType.GetCustomAttribute<ScriptModuleAttribute>();

            if (scriptModuleAttribute is null)
            {
                continue;
            }

            if (!_serviceProvider.IsRegistered(module.ModuleType))
            {
                _serviceProvider.Register(module.ModuleType, Reuse.Singleton);
            }

            var instance = _serviceProvider.GetService(module.ModuleType);

            if (instance is null)
            {
                throw new InvalidOperationException($"Unable to create instance of script module {module.ModuleType.Name}");
            }

            var moduleName = scriptModuleAttribute.Name;
            _logger.Debug("Registering script module {Name}", moduleName);

            // Register the type with MoonSharp
            UserData.RegisterType(module.ModuleType, InteropAccessMode.Reflection);

            // Create a table for the module
            var moduleTable = CreateModuleTable(instance, module.ModuleType);
            LuaScript.Globals[moduleName] = moduleTable;

            _loadedModules[moduleName] = instance;
        }

        RegisterEnums();
    }
}
