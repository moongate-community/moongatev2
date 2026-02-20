using Moongate.Core.Json;
using Moongate.UO.Data.Expansions;
using Moongate.UO.Data.Json;
using Moongate.UO.Data.Json.Context;
using Moongate.UO.Data.Json.Names;
using Moongate.UO.Data.Json.Regions;
using Moongate.UO.Data.Json.Weather;
using Moongate.UO.Data.Skills;

namespace Moongate.Tests.UO.Data.Json;

public class MoongateUOJsonSerializationContextTests
{
    [Test]
    public void Context_ShouldRegister_AllJsonRootTypesUsedByLoaders()
    {
        var context = MoongateUOJsonSerializationContext.Default;

        Assert.Multiple(
            () =>
            {
                Assert.That(JsonContextTypeResolver.IsTypeRegistered(context, typeof(SkillInfo[])), Is.True);
                Assert.That(JsonContextTypeResolver.IsTypeRegistered(context, typeof(ExpansionInfo[])), Is.True);
                Assert.That(JsonContextTypeResolver.IsTypeRegistered(context, typeof(JsonContainerSize[])), Is.True);
                Assert.That(JsonContextTypeResolver.IsTypeRegistered(context, typeof(JsonNameDef[])), Is.True);
                Assert.That(JsonContextTypeResolver.IsTypeRegistered(context, typeof(JsonRegionWrap)), Is.True);
                Assert.That(JsonContextTypeResolver.IsTypeRegistered(context, typeof(JsonWeatherWrap)), Is.True);
                Assert.That(JsonContextTypeResolver.IsTypeRegistered(context, typeof(JsonProfessionsRoot)), Is.True);
            }
        );
    }

    [Test]
    public void Deserialize_AssetJsonFiles_ShouldSucceedForAllLoaderTargets()
    {
        var dataRoot = GetAssetsDataRoot();
        var containersFile = Path.Combine(dataRoot, "containers", "default_containers.json");
        var namesFile = Path.Combine(dataRoot, "names", "modernuo_names.json");
        var regionsFile = Path.Combine(dataRoot, "regions", "regions.json");
        var weatherFile = Path.Combine(dataRoot, "weather", "weather.json");
        var expansionsFile = Path.Combine(dataRoot, "expansions.json");
        var skillsFile = Path.Combine(dataRoot, "skills.json");
        var professionsFile = Path.Combine(dataRoot, "Professions", "professions.json");

        var context = MoongateUOJsonSerializationContext.Default;
        var containers = JsonUtils.DeserializeFromFile<JsonContainerSize[]>(containersFile, context);
        var names = JsonUtils.DeserializeFromFile<JsonNameDef[]>(namesFile, context);
        var regions = JsonUtils.DeserializeFromFile<JsonRegionWrap>(regionsFile, context);
        var weather = JsonUtils.DeserializeFromFile<JsonWeatherWrap>(weatherFile, context);
        var expansions = JsonUtils.DeserializeFromFile<ExpansionInfo[]>(expansionsFile, context);
        var skills = JsonUtils.DeserializeFromFile<SkillInfo[]>(skillsFile, context);
        var professions = JsonUtils.DeserializeFromFile<JsonProfessionsRoot>(professionsFile, context);

        Assert.Multiple(
            () =>
            {
                Assert.That(containers, Is.Not.Null);
                Assert.That(containers.Length, Is.GreaterThan(0));
                Assert.That(containers.All(static container => !string.IsNullOrWhiteSpace(container.Id)), Is.True);
                Assert.That(names, Is.Not.Null);
                Assert.That(names.Length, Is.GreaterThan(0));
                Assert.That(regions, Is.Not.Null);
                Assert.That(regions.Regions.Count, Is.GreaterThan(0));
                Assert.That(weather, Is.Not.Null);
                Assert.That(weather.WeatherTypes.Count, Is.GreaterThan(0));
                Assert.That(expansions, Is.Not.Null);
                Assert.That(expansions.Length, Is.GreaterThan(0));
                Assert.That(skills, Is.Not.Null);
                Assert.That(skills.Length, Is.GreaterThan(0));
                Assert.That(professions, Is.Not.Null);
                Assert.That(professions.Professions.Length, Is.GreaterThan(0));
            }
        );
    }

    [OneTimeSetUp]
    public void Setup()
        => JsonUtils.RegisterJsonContext(MoongateUOJsonSerializationContext.Default);

    private static string GetAssetsDataRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "src", "Moongate.Server", "Assets", "data");

            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate src/Moongate.Server/Assets/data from test base directory.");
    }
}
