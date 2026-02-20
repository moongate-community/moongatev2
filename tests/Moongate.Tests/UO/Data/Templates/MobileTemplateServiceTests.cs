using Moongate.UO.Data.Services.Templates;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.Tests.UO.Data.Templates;

public class MobileTemplateServiceTests
{
    private MobileTemplateService _service = null!;

    [Test]
    public void Clear_ShouldRemoveTemplates()
    {
        _service.Upsert(CreateDefinition("orc", "Orc"));

        _service.Clear();

        Assert.That(_service.Count, Is.Zero);
    }

    [SetUp]
    public void SetUp()
        => _service = new();

    [Test]
    public void Upsert_ShouldRegisterTemplate()
    {
        var definition = CreateDefinition("orc", "Orc");

        _service.Upsert(definition);

        Assert.Multiple(
            () =>
            {
                Assert.That(_service.Count, Is.EqualTo(1));
                Assert.That(_service.TryGet("orc", out var resolved), Is.True);
                Assert.That(resolved?.Name, Is.EqualTo("Orc"));
            }
        );
    }

    [Test]
    public void UpsertRange_ShouldRegisterTemplates()
    {
        _service.UpsertRange([CreateDefinition("orc", "Orc"), CreateDefinition("rat", "Rat")]);

        Assert.That(_service.Count, Is.EqualTo(2));
    }

    private static MobileTemplateDefinition CreateDefinition(string id, string name)
        => new()
        {
            Id = id,
            Name = name,
            Category = "monsters",
            Description = name,
            Body = 0x11,
            SkinHue = HueSpec.FromRange(779, 790),
            HairHue = HueSpec.FromValue(0),
            Brain = "test"
        };
}
