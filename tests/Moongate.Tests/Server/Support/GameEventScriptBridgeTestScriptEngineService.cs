using Moongate.Scripting.Data.Scripts;
using Moongate.Scripting.Interfaces;

namespace Moongate.Tests.Server.Support;

public sealed class GameEventScriptBridgeTestScriptEngineService : IScriptEngineService
{
    public string? LastCallbackName { get; private set; }
    public object[]? LastCallbackArgs { get; private set; }
    public void AddCallback(string name, Action<object[]> callback) { }
    public void AddConstant(string name, object value) { }
    public void AddInitScript(string script) { }
    public void AddManualModuleFunction(string moduleName, string functionName, Action<object[]> callback) { }

    public void AddManualModuleFunction<TInput, TOutput>(
        string moduleName,
        string functionName,
        Func<TInput?, TOutput> callback
    ) { }

    public void AddScriptModule(Type type) { }

    public void CallFunction(string functionName, params object[] args)
    {
        LastCallbackName = functionName;
        LastCallbackArgs = args;
    }

    public void ClearScriptCache() { }

    public void ExecuteCallback(string name, params object[] args)
    {
        LastCallbackName = name;
        LastCallbackArgs = args;
    }

    public void ExecuteEngineReady() { }

    public ScriptResult ExecuteFunction(string command)
        => new() { Success = true };

    public Task<ScriptResult> ExecuteFunctionAsync(string command)
        => Task.FromResult(new ScriptResult { Success = true });

    public void ExecuteFunctionFromBootstrap(string name) { }
    public void ExecuteScript(string script) { }
    public void ExecuteScriptFile(string scriptFile) { }

    public ScriptExecutionMetrics GetExecutionMetrics()
        => new();

    public void RegisterGlobal(string name, object value) { }
    public void RegisterGlobalFunction(string name, Delegate func) { }

    public Task StartAsync()
        => Task.CompletedTask;

    public Task StopAsync()
        => Task.CompletedTask;

    public string ToScriptEngineFunctionName(string name)
        => name;

    public bool UnregisterGlobal(string name)
        => true;

#pragma warning disable CS0067
    public event IScriptEngineService.LuaFileChangedHandler? FileChanged;
    public event EventHandler<ScriptErrorInfo>? OnScriptError;
#pragma warning restore CS0067
}
