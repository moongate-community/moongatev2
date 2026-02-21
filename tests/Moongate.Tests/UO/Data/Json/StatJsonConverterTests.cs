using System.Text.Json;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.UO.Data.Json;

public class StatJsonConverterTests
{
    [TestCase("\"Str\"", Stat.Strength)]
    [TestCase("\"Dex\"", Stat.Dexterity)]
    [TestCase("\"Int\"", Stat.Intelligence)]
    [TestCase("\"Strength\"", Stat.Strength)]
    [TestCase("\"Dexterity\"", Stat.Dexterity)]
    [TestCase("\"Intelligence\"", Stat.Intelligence)]
    [TestCase("0", Stat.Strength)]
    [TestCase("1", Stat.Dexterity)]
    [TestCase("2", Stat.Intelligence)]
    public void Deserialize_ShouldSupportAliasesAndEnumValues(string json, Stat expected)
    {
        var value = JsonSerializer.Deserialize<Stat>(json);

        Assert.That(value, Is.EqualTo(expected));
    }

    [Test]
    public void Deserialize_ShouldThrow_OnUnknownValue()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Stat>("\"Foo\""));
    }
}
