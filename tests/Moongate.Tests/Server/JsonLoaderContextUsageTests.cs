using System.Text.RegularExpressions;

namespace Moongate.Tests.Server;

public class JsonLoaderContextUsageTests
{
    [Test]
    public void JsonLoaders_ShouldUseUoJsonContextForEachDeserializeFromFileCall()
    {
        var repoRoot = GetRepositoryRoot();

        foreach (var filePath in GetJsonLoaderFilePaths())
        {
            var fullPath = Path.Combine(repoRoot, filePath);
            var content = File.ReadAllText(fullPath);

            var deserializeCalls = Regex.Count(content, @"JsonUtils\.DeserializeFromFile<[^>]+>\s*\(");
            var contextCalls = Regex.Count(content, @"MoongateUOJsonSerializationContext\.Default");

            Assert.Multiple(
                () =>
                {
                    Assert.That(deserializeCalls, Is.GreaterThan(0), $"{fullPath} should deserialize at least one JSON file.");
                    Assert.That(
                        contextCalls,
                        Is.GreaterThanOrEqualTo(deserializeCalls),
                        $"{fullPath} should pass MoongateUOJsonSerializationContext.Default to every JSON deserialize call."
                    );
                }
            );
        }
    }

    private static string GetRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Moongate.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate repository root from test base directory.");
    }

    private static IReadOnlyList<string> GetJsonLoaderFilePaths()
        =>
        [
            "src/Moongate.Server/FileLoaders/ContainersDataLoader.cs",
            "src/Moongate.Server/FileLoaders/ExpansionLoader.cs",
            "src/Moongate.Server/FileLoaders/NamesLoader.cs",
            "src/Moongate.Server/FileLoaders/ProfessionsLoader.cs",
            "src/Moongate.Server/FileLoaders/RegionDataLoader.cs",
            "src/Moongate.Server/FileLoaders/SkillLoader.cs",
            "src/Moongate.Server/FileLoaders/WeatherDataLoader.cs"
        ];
}
