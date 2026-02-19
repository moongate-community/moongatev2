using Moongate.Scripting.Loaders;
using Moongate.Tests.TestSupport;
using MoonSharp.Interpreter;

namespace Moongate.Tests.Scripting;

public class LuaScriptLoaderTests
{
    [Test]
    public void LoadFile_WhenModuleDoesNotExist_ShouldReturnNull()
    {
        using var temp = new TempDirectory();
        var loader = new LuaScriptLoader(temp.Path);
        var table = new Table(new());

        var content = loader.LoadFile("missing_module", table);

        Assert.That(content, Is.Null);
    }

    [Test]
    public void LoadFile_WhenModuleExists_ShouldReturnContent()
    {
        using var temp = new TempDirectory();
        var scriptsDir = temp.Path;
        var expected = "return 99";
        File.WriteAllText(Path.Combine(scriptsDir, "math_module.lua"), expected);
        var loader = new LuaScriptLoader(scriptsDir);
        var table = new Table(new());

        var content = loader.LoadFile("math_module", table);

        Assert.That(content, Is.EqualTo(expected));
    }

    [Test]
    public void ScriptFileExists_WhenModuleExists_ShouldReturnTrue()
    {
        using var temp = new TempDirectory();
        var scriptsDir = temp.Path;
        File.WriteAllText(Path.Combine(scriptsDir, "test_module.lua"), "return 42");
        var loader = new LuaScriptLoader(scriptsDir);

        var exists = loader.ScriptFileExists("test_module");

        Assert.That(exists, Is.True);
    }
}
