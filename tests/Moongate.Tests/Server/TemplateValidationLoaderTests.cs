using Moongate.Server.FileLoaders;
using Moongate.UO.Data.Containers;
using Moongate.UO.Data.Services.Templates;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Mobiles;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Server;

public class TemplateValidationLoaderTests
{
    [SetUp]
    public void SetUp()
    {
        ContainerLayoutSystem.ContainerSizes.Clear();
        ContainerLayoutSystem.ContainerSizesById.Clear();
    }

    [Test]
    public async Task LoadAsync_WhenTemplatesAreValid_ShouldNotThrow()
    {
        var itemService = new ItemTemplateService();
        var mobileService = new MobileTemplateService();
        ContainerLayoutSystem.ContainerSizesById["backpack"] = new("backpack", 7, 4, "Backpack");

        itemService.Upsert(
            new()
            {
                Id = "item.shirt",
                Name = "Shirt",
                Category = "clothes",
                Description = "shirt",
                ItemId = "0x1517",
                Hue = HueSpec.FromRange(5, 55),
                GoldValue = GoldValueSpec.FromDiceExpression("1d8+8"),
                LootType = LootType.Regular,
                ScriptId = "none",
                Weight = 1,
                Tags = ["container"],
                ContainerLayoutId = "backpack"
            }
        );

        mobileService.Upsert(
            new()
            {
                Id = "orc",
                Name = "Orc",
                Category = "monsters",
                Description = "orc",
                Body = 0x11,
                SkinHue = HueSpec.FromValue(779),
                HairHue = HueSpec.FromValue(0),
                FixedEquipment =
                [
                    new()
                    {
                        ItemTemplateId = "item.shirt",
                        Layer = ItemLayerType.Shirt
                    }
                ]
            }
        );

        var loader = new TemplateValidationLoader(itemService, mobileService);

        Assert.That(async () => await loader.LoadAsync(), Throws.Nothing);
    }

    [Test]
    public void LoadAsync_WhenMobileReferencesMissingItem_ShouldThrow()
    {
        var itemService = new ItemTemplateService();
        var mobileService = new MobileTemplateService();

        mobileService.Upsert(
            new()
            {
                Id = "orc",
                Name = "Orc",
                Category = "monsters",
                Description = "orc",
                Body = 0x11,
                SkinHue = HueSpec.FromValue(779),
                HairHue = HueSpec.FromValue(0),
                FixedEquipment =
                [
                    new()
                    {
                        ItemTemplateId = "item.missing",
                        Layer = ItemLayerType.Shirt
                    }
                ]
            }
        );

        var loader = new TemplateValidationLoader(itemService, mobileService);

        Assert.ThrowsAsync<InvalidOperationException>(async () => await loader.LoadAsync());
    }

    [Test]
    public void LoadAsync_WhenContainerItemHasMissingLayoutId_ShouldThrow()
    {
        var itemService = new ItemTemplateService();
        var mobileService = new MobileTemplateService();

        itemService.Upsert(
            new()
            {
                Id = "item.container",
                Name = "Container",
                Category = "container",
                Description = "container",
                ItemId = "0x0E76",
                Hue = HueSpec.FromValue(0),
                GoldValue = GoldValueSpec.FromValue(0),
                LootType = LootType.Regular,
                ScriptId = "none",
                Weight = 1,
                Tags = ["container"]
            }
        );

        var loader = new TemplateValidationLoader(itemService, mobileService);

        Assert.ThrowsAsync<InvalidOperationException>(async () => await loader.LoadAsync());
    }
}
