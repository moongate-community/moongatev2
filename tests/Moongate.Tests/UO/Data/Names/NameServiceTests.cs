using Moongate.UO.Data.Services.Names;
using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.Tests.UO.Data.Names;

public class NameServiceTests
{
    private NameService _service = null!;

    [Test]
    public void GenerateName_ForTemplate_ShouldUseIdThenCategoryThenName()
    {
        _service.AddNames("orc_warrior", "Gor");

        var template = new MobileTemplateDefinition
        {
            Id = "orc_warrior",
            Category = "orc",
            Name = "Orc Warrior"
        };

        var name = _service.GenerateName(template);

        Assert.That(name, Is.EqualTo("Gor"));
    }

    [Test]
    public void GenerateName_WhenTypeHasValues_ShouldReturnName()
    {
        _service.AddNames("tokuno male", "Dosyaku", "Warimoto");

        var name = _service.GenerateName("tokuno male");

        Assert.That(name, Is.Not.Empty);
    }

    [Test]
    public void GenerateName_WhenTypeMissing_ShouldReturnEmpty()
    {
        var name = _service.GenerateName("missing");

        Assert.That(name, Is.Empty);
    }

    [SetUp]
    public void SetUp()
        => _service = new();
}
