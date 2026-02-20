using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Network.Packets.Incoming.Login;
using Moongate.Network.Spans;
using Moongate.Server.Services.Entities;
using Moongate.Server.Services.Persistence;
using Moongate.Server.Services.Timing;
using Moongate.Tests.TestSupport;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Services.Names;
using Moongate.UO.Data.Services.Templates;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Server;

public class EntityFactoryServiceTests
{
    [Test]
    public async Task CreateItemFromTemplate_ShouldMapTemplateAndAllocateItemSerial()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var itemTemplateService = new ItemTemplateService();
        itemTemplateService.Upsert(
            new()
            {
                Id = "item.shirt",
                Name = "Shirt",
                Category = "clothes",
                Description = "test",
                ItemId = "0x1517",
                Hue = HueSpec.FromValue(100),
                GoldValue = GoldValueSpec.FromValue(1),
                LootType = LootType.Regular,
                ScriptId = "none"
            }
        );

        var service = new EntityFactoryService(
            itemTemplateService,
            new MobileTemplateService(),
            new NameService(),
            persistence
        );

        var item = service.CreateItemFromTemplate("item.shirt");

        Assert.Multiple(
            () =>
            {
                Assert.That(item.Id.IsItem, Is.True);
                Assert.That(item.ItemId, Is.EqualTo(0x1517));
                Assert.That(item.Hue, Is.EqualTo(100));
            }
        );
    }

    [Test]
    public async Task CreateMobileFromTemplate_ShouldUseNameServiceAndAllocateMobileSerial()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var nameService = new NameService();
        nameService.AddNames("orc_warrior", "Gor");

        var mobileTemplateService = new MobileTemplateService();
        mobileTemplateService.Upsert(
            new()
            {
                Id = "orc_warrior",
                Name = "Orc Warrior",
                Category = "orc",
                Description = "orc",
                Body = 0x11,
                SkinHue = HueSpec.FromValue(1000),
                HairHue = HueSpec.FromValue(0),
                HairStyle = 0,
                Strength = 70,
                Dexterity = 60,
                Intelligence = 20,
                Hits = 70,
                Mana = 20,
                Stamina = 60,
                Brain = "aggressive_orc"
            }
        );

        var service = new EntityFactoryService(
            new ItemTemplateService(),
            mobileTemplateService,
            nameService,
            persistence
        );

        var mobile = service.CreateMobileFromTemplate("orc_warrior");

        Assert.Multiple(
            () =>
            {
                Assert.That(mobile.Id.IsMobile, Is.True);
                Assert.That(mobile.Name, Is.EqualTo("Gor"));
                Assert.That(mobile.Strength, Is.EqualTo(70));
                Assert.That(mobile.Hits, Is.EqualTo(70));
                Assert.That(mobile.IsPlayer, Is.False);
            }
        );
    }

    [Test]
    public async Task CreatePlayerMobile_ShouldMapPacketAndAllocateMobileSerial()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var packet = new CharacterCreationPacket();
        _ = packet.TryParse(BuildCharacterCreationPayload());

        var service = new EntityFactoryService(
            new ItemTemplateService(),
            new MobileTemplateService(),
            new NameService(),
            persistence
        );

        var mobile = service.CreatePlayerMobile(packet, (Serial)0x00000101);

        Assert.Multiple(
            () =>
            {
                Assert.That(mobile.Id.IsMobile, Is.True);
                Assert.That(mobile.AccountId, Is.EqualTo((Serial)0x00000101));
                Assert.That(mobile.Name, Is.EqualTo("TestCharacter"));
                Assert.That(mobile.Gender, Is.EqualTo(GenderType.Female));
                Assert.That(mobile.ProfessionId, Is.EqualTo(2));
                Assert.That(mobile.IsPlayer, Is.True);
            }
        );
    }

    [Test]
    public async Task GetNewBackpack_ShouldReturnBackpackFromTemplate_WhenAvailable()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var itemTemplateService = new ItemTemplateService();
        itemTemplateService.Upsert(
            new()
            {
                Id = "backpack",
                Name = "Backpack",
                Category = "containers",
                Description = "Backpack",
                ItemId = "0x0E75",
                Hue = HueSpec.FromValue(0),
                GoldValue = GoldValueSpec.FromValue(0),
                LootType = LootType.Regular,
                ScriptId = "none"
            }
        );

        var service = new EntityFactoryService(
            itemTemplateService,
            new MobileTemplateService(),
            new NameService(),
            persistence
        );

        var backpack = service.GetNewBackpack();

        Assert.Multiple(
            () =>
            {
                Assert.That(backpack.Id.IsItem, Is.True);
                Assert.That(backpack.ItemId, Is.EqualTo(0x0E75));
            }
        );
    }

    private static byte[] BuildCharacterCreationPayload()
    {
        var writer = new SpanWriter(106, true);

        writer.Write((byte)0xF8);
        writer.Write(unchecked((int)0xEDEDEDED));
        writer.Write(unchecked((int)0xFFFFFFFF));
        writer.Write((byte)0x00);
        writer.WriteAscii("TestCharacter", 30);
        writer.Write((ushort)0);
        writer.Write((uint)ClientFlags.Trammel);
        writer.Write(0);
        writer.Write(0);
        writer.Write((byte)2);
        writer.Clear(15);
        writer.Write((byte)5);
        writer.Write((byte)60);
        writer.Write((byte)50);
        writer.Write((byte)40);
        writer.Write((byte)UOSkillName.Magery);
        writer.Write((byte)50);
        writer.Write((byte)UOSkillName.Meditation);
        writer.Write((byte)50);
        writer.Write((byte)UOSkillName.EvalInt);
        writer.Write((byte)50);
        writer.Write((byte)UOSkillName.Wrestling);
        writer.Write((byte)50);
        writer.Write((short)0x0455);
        writer.Write((short)0x0203);
        writer.Write((short)0x0304);
        writer.Write((short)0x0506);
        writer.Write((short)0x0708);
        writer.Write((short)0);
        writer.Write((ushort)0);
        writer.Write((short)1);
        writer.Write(0);
        writer.Write((short)0x0888);
        writer.Write((short)0x0999);

        var result = writer.ToArray();
        writer.Dispose();

        return result;
    }

    private static async Task<PersistenceService> CreatePersistenceServiceAsync(string rootDirectory)
    {
        var directories = new DirectoriesConfig(rootDirectory, Enum.GetNames<DirectoryType>());
        var persistence = new PersistenceService(
            directories,
            new TimerWheelService(
                new()
                {
                    TickDuration = TimeSpan.FromMilliseconds(250),
                    WheelSize = 512
                }
            ),
            new()
        );

        await persistence.StartAsync();

        return persistence;
    }
}
