using System.Text.Json;
using Moongate.UO.Data.Services.Templates;

namespace Moongate.Tests.UO.Data.Templates;

public class StartupTemplateServiceTests
{
    private StartupTemplateService _service = null!;

    [Test]
    public void Clear_ShouldRemoveAllDocuments()
    {
        using var document = JsonDocument.Parse("""{ "startingGold": 1000 }""");
        var json = document.RootElement;
        _service.Upsert("startup_base", json);

        _service.Clear();

        Assert.That(_service.Count, Is.Zero);
    }

    [SetUp]
    public void SetUp()
        => _service = new();

    [Test]
    public void Upsert_ShouldRegisterDocument()
    {
        using var document = JsonDocument.Parse("""{ "startingGold": 1000 }""");
        var json = document.RootElement;

        _service.Upsert("startup_base", json);

        Assert.Multiple(
            () =>
            {
                Assert.That(_service.Count, Is.EqualTo(1));
                Assert.That(_service.TryGet("startup_base", out var document), Is.True);
                Assert.That(document?.Content.GetProperty("startingGold").GetInt32(), Is.EqualTo(1000));
            }
        );
    }

    [Test]
    public void Upsert_WhenIdAlreadyExists_ShouldReplaceDocument()
    {
        using var firstDocument = JsonDocument.Parse("""{ "value": 1 }""");
        using var secondDocument = JsonDocument.Parse("""{ "value": 2 }""");
        var first = firstDocument.RootElement;
        var second = secondDocument.RootElement;

        _service.Upsert("startup_base", first);
        _service.Upsert("startup_base", second);

        _service.TryGet("startup_base", out var document);

        Assert.Multiple(
            () =>
            {
                Assert.That(_service.Count, Is.EqualTo(1));
                Assert.That(document?.Content.GetProperty("value").GetInt32(), Is.EqualTo(2));
            }
        );
    }
}
