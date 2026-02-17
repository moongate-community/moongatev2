using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using Serilog;

namespace Moongate.Scripting.Loaders;

/// <summary>
/// Custom script loader for MoonSharp that loads Lua modules from the configured Scripts directory.
/// Implements the MoonSharp script loader interface to provide require() functionality.
/// </summary>
public class LuaScriptLoader : ScriptLoaderBase
{
    private readonly ILogger _logger = Log.ForContext<LuaScriptLoader>();
    private readonly string _scriptsDirectory;

    /// <summary>
    /// Initializes a new instance of the LuaScriptLoader class.
    /// </summary>
    /// <param name="directoriesConfig">The directories configuration to resolve the scripts directory.</param>
    public LuaScriptLoader(string rootDirectory)
    {
        ArgumentNullException.ThrowIfNull(rootDirectory);

        _scriptsDirectory = rootDirectory;

        // Configure default module search paths
        ModulePaths =
        [
            "?.lua",
            "?/init.lua",
            "modules/?.lua",
            "modules/?/init.lua"
        ];

        _logger.Debug("Lua script loader initialized with scripts directory: {ScriptsDirectory}", _scriptsDirectory);
    }

    /// <summary>
    /// Loads a Lua script file from the configured scripts directory.
    /// </summary>
    /// <param name="file">The filename or module name to load.</param>
    /// <param name="globalContext">The global context table.</param>
    /// <returns>The script content as a string, or null if the file doesn't exist.</returns>
    public override object LoadFile(string file, Table globalContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(file);

        // Remove .lua extension if present (MoonSharp sometimes adds it)
        // This matches the behavior of ScriptFileExists
        file = file.Replace(".lua", string.Empty);

        var resolvedPath = ResolveModulePath(file);

        if (resolvedPath == null)
        {
            _logger.Warning("Script file not found: {FileName}", file);

            return null;
        }

        try
        {
            var content = File.ReadAllText(resolvedPath);
            _logger.Debug("Loaded script file: {FileName} from {Path}", file, resolvedPath);

            return content;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load script file: {FileName}", file);

            throw new ScriptRuntimeException($"Failed to load module '{file}': {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if a script file exists in the configured scripts directory.
    /// </summary>
    /// <param name="name">The filename or module name to check.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    public override bool ScriptFileExists(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        name = name.Replace(".lua", string.Empty);
        var resolvedPath = ResolveModulePath(name);

        return resolvedPath != null;
    }

    /// <summary>
    /// Resolves a module name to a full file path by searching through configured module paths.
    /// </summary>
    /// <param name="moduleName">The module name to resolve.</param>
    /// <returns>The full path to the module file, or null if not found.</returns>
    private string? ResolveModulePath(string moduleName)
    {
        // Try each module path pattern
        foreach (var pattern in ModulePaths)
        {
            var fileName = pattern.Replace("?", moduleName);
            var fullPath = Path.Combine(_scriptsDirectory, fileName);

            if (File.Exists(fullPath))
            {
                _logger.Debug("Resolved module '{ModuleName}' to path: {FullPath}", moduleName, fullPath);

                return fullPath;
            }
        }

        // If no pattern matched, try the direct path
        var directPath = Path.Combine(_scriptsDirectory, moduleName);

        if (File.Exists(directPath))
        {
            _logger.Debug("Resolved module '{ModuleName}' to direct path: {DirectPath}", moduleName, directPath);

            return directPath;
        }

        return null;
    }
}
