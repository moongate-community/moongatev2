using System.Text.Json;
using Moongate.Core.Json;
using Moongate.UO.Data.Json.Context;
using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.Tests.UO.Data.Templates;

public class MobileTemplatePolymorphicTests
{
    [Test]
    public void Context_ShouldRegister_MobileTemplateRootTypes()
    {
        var context = MoongateUOTemplateJsonContext.Default;

        Assert.Multiple(() =>
        {
            Assert.That(JsonContextTypeResolver.IsTypeRegistered(context, typeof(MobileTemplateDefinitionBase[])), Is.True);
            Assert.That(JsonContextTypeResolver.IsTypeRegistered(context, typeof(MobileTemplateDefinition[])), Is.True);
        });
    }

    [Test]
    public void Deserialize_WithPolymorphicTypeMobile_ShouldCreateMobileTemplateDefinition()
    {
        var json = """
        [
          {
            "type": "mobile",
            "id": "orc_warrior",
            "name": "Orc Warrior",
            "category": "monsters",
            "description": "A tough orc fighter",
            "tags": ["orc", "melee"],
            "body": "0x11",
            "skinHue": "hue(779:790)",
            "hairHue": "hue(1100:1120)",
            "hairStyle": 0,
            "strength": 70,
            "dexterity": 45,
            "intelligence": 25,
            "hits": 120,
            "mana": 25,
            "stamina": 80,
            "brain": "aggressive_orc",
            "fixedEquipment": [],
            "randomEquipment": []
          }
        ]
        """;

        var deserialized = JsonSerializer.Deserialize(
            json,
            MoongateUOTemplateJsonContext.Default.GetTypeInfo(typeof(MobileTemplateDefinitionBase[]))
        );
        var result = deserialized as MobileTemplateDefinitionBase[];

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Length, Is.EqualTo(1));
            Assert.That(result[0], Is.TypeOf<MobileTemplateDefinition>());
            var mobile = (MobileTemplateDefinition)result[0];
            Assert.That(mobile.Id, Is.EqualTo("orc_warrior"));
            Assert.That(mobile.Body, Is.EqualTo(0x11));
            Assert.That(mobile.SkinHue.IsRange, Is.True);
            Assert.That(mobile.HairHue.IsRange, Is.True);
            Assert.That(mobile.Brain, Is.EqualTo("aggressive_orc"));
        });
    }
}
