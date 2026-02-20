using Moongate.UO.Data.Services.Templates;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.UO.Data.Templates;

public class ItemTemplateServiceTests
{
    private ItemTemplateService _service = null!;

    [SetUp]
    public void SetUp()
        => _service = new ItemTemplateService();

    [Test]
    public void Upsert_ShouldRegisterTemplate()
    {
        var template = CreateTemplate("item.shirt", "Shirt");

        _service.Upsert(template);

        Assert.Multiple(() =>
        {
            Assert.That(_service.Count, Is.EqualTo(1));
            Assert.That(_service.TryGet("item.shirt", out var resolved), Is.True);
            Assert.That(resolved?.Name, Is.EqualTo("Shirt"));
        });
    }

    [Test]
    public void Upsert_WhenIdAlreadyExists_ShouldReplaceTemplate()
    {
        _service.Upsert(CreateTemplate("item.shirt", "Old Shirt"));
        _service.Upsert(CreateTemplate("item.shirt", "New Shirt"));

        _service.TryGet("item.shirt", out var resolved);

        Assert.Multiple(() =>
        {
            Assert.That(_service.Count, Is.EqualTo(1));
            Assert.That(resolved?.Name, Is.EqualTo("New Shirt"));
        });
    }

    [Test]
    public void UpsertRange_ShouldRegisterAllTemplates()
    {
        _service.UpsertRange([CreateTemplate("item.shirt", "Shirt"), CreateTemplate("item.pants", "Pants")]);

        Assert.That(_service.Count, Is.EqualTo(2));
    }

    [Test]
    public void Clear_ShouldRemoveAllTemplates()
    {
        _service.Upsert(CreateTemplate("item.shirt", "Shirt"));

        _service.Clear();

        Assert.That(_service.Count, Is.Zero);
    }

    private static ItemTemplateDefinition CreateTemplate(string id, string name)
        => new()
        {
            Id = id,
            Name = name,
            Category = "clothes",
            Description = name,
            GoldValue = GoldValueSpec.FromValue(0),
            Hue = HueSpec.FromValue(0),
            ItemId = "0x1517",
            LootType = LootType.Regular,
            ScriptId = "none"
        };
}
