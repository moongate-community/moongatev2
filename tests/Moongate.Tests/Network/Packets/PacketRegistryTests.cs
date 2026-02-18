using Moongate.Network.Packets.Incoming.House;
using Moongate.Network.Packets.Incoming.Interaction;
using Moongate.Network.Packets.Incoming.Login;
using Moongate.Network.Packets.Incoming.Movement;
using Moongate.Network.Packets.Incoming.Speech;
using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Interfaces;
using Moongate.Network.Packets.Outgoing.Entity;
using Moongate.Network.Packets.Outgoing.Movement;
using Moongate.Network.Packets.Registry;
using Moongate.Network.Packets.Types.Packets;

namespace Moongate.Tests.Network.Packets;

public class PacketRegistryTests
{
    [Test]
    public void PacketTable_Register_ShouldRegisterKnownPackets()
    {
        var registry = new PacketRegistry();

        PacketTable.Register(registry);

        Assert.Multiple(
            () =>
            {
                Assert.That(registry.TryCreatePacket(0xEF, out var packetEf), Is.True);
                Assert.That(packetEf, Is.TypeOf<LoginSeedPacket>());

                Assert.That(registry.TryGetDescriptor(0x78, out var mobileIncomingDescriptor), Is.True);
                Assert.That(mobileIncomingDescriptor.Sizing, Is.EqualTo(PacketSizing.Variable));
                Assert.That(mobileIncomingDescriptor.HandlerType, Is.EqualTo(typeof(MobileIncomingPacket)));

                Assert.That(registry.TryGetDescriptor(0x08, out var dropDescriptor), Is.True);
                Assert.That(dropDescriptor.Length, Is.EqualTo(14));
                Assert.That(dropDescriptor.Description, Is.EqualTo("Drop Item"));

                Assert.That(registry.TryCreatePacket(0xFB, out var packetFb), Is.True);
                Assert.That(packetFb, Is.TypeOf<UpdateViewPublicHouseContentsPacket>());
            }
        );
    }

    [Test]
    public void Register_WhenOpcodeAlreadyRegistered_ShouldThrow()
    {
        var registry = new PacketRegistry();
        registry.RegisterFixed<MoveRequestPacket>(0x02, 7, "Move Request");

        Assert.That(() => registry.RegisterFixed<DoubleClickPacket>(0x02, 5), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void RegisteredPackets_ShouldReturnAllDescriptorsOrderedByOpcode()
    {
        var registry = new PacketRegistry();
        registry.RegisterVariable<UnicodeSpeechPacket>(0xAD);
        registry.RegisterFixed<MoveRequestPacket>(0x02, 7);
        registry.RegisterFixed<DoubleClickPacket>(0x06, 5);

        var packets = registry.RegisteredPackets;

        Assert.That(packets.Count, Is.EqualTo(3));
        Assert.That(packets.Select(static p => p.OpCode), Is.EqualTo(new byte[] { 0x02, 0x06, 0xAD }));
        Assert.That(packets[0].HandlerType, Is.EqualTo(typeof(MoveRequestPacket)));
        Assert.That(packets[0].Length, Is.EqualTo(7));
    }

    [Test]
    public void RegisterFixed_ShouldStoreDescriptorAndCreatePacket()
    {
        var registry = new PacketRegistry();

        registry.RegisterFixed<MoveRequestPacket>(0x02, 7, "Move Request");

        var hasDescriptor = registry.TryGetDescriptor(0x02, out var descriptor);
        var hasPacket = registry.TryCreatePacket(0x02, out var packet);

        Assert.That(hasDescriptor, Is.True);
        Assert.That(descriptor.OpCode, Is.EqualTo(0x02));
        Assert.That(descriptor.Sizing, Is.EqualTo(PacketSizing.Fixed));
        Assert.That(descriptor.Length, Is.EqualTo(7));
        Assert.That(descriptor.Description, Is.EqualTo("Move Request"));
        Assert.That(descriptor.HandlerType, Is.EqualTo(typeof(MoveRequestPacket)));
        Assert.That(hasPacket, Is.True);
        Assert.That(packet, Is.TypeOf<MoveRequestPacket>());
    }

    [Test]
    public void RegisterFromAttribute_ShouldReadAttributeMetadata()
    {
        var registry = new PacketRegistry();

        registry.RegisterFromAttribute<MoveConfirmPacket>();

        var hasDescriptor = registry.TryGetDescriptor(0x22, out var descriptor);

        Assert.That(hasDescriptor, Is.True);
        Assert.That(descriptor.Length, Is.EqualTo(3));
    }

    [Test]
    public void RegisterVariable_ShouldStoreVariableDescriptor()
    {
        var registry = new PacketRegistry();

        registry.RegisterVariable<UnicodeSpeechPacket>(0xAD, "Unicode/Ascii speech request");

        var hasDescriptor = registry.TryGetDescriptor(0xAD, out var descriptor);

        Assert.That(hasDescriptor, Is.True);
        Assert.That(descriptor.Sizing, Is.EqualTo(PacketSizing.Variable));
        Assert.That(descriptor.Length, Is.EqualTo(-1));
        Assert.That(descriptor.Description, Is.EqualTo("Unicode/Ascii speech request"));
    }

    [Test]
    public void PacketTable_Register_ShouldMatchAllPacketHandlerAttributes()
    {
        var registry = new PacketRegistry();
        PacketTable.Register(registry);

        var packetTypes = typeof(PacketTable).Assembly
                                             .GetTypes()
                                             .Where(
                                                 static type =>
                                                     !type.IsAbstract &&
                                                     typeof(IGameNetworkPacket).IsAssignableFrom(type)
                                             )
                                             .Select(
                                                 static type =>
                                                     (
                                                         Type: type,
                                                         Attribute: type.GetCustomAttributes(
                                                                             typeof(PacketHandlerAttribute),
                                                                             false
                                                                         )
                                                                        .OfType<PacketHandlerAttribute>()
                                                                        .SingleOrDefault()
                                                     )
                                             )
                                             .Where(static x => x.Attribute is not null)
                                             .Select(static x => (x.Type, Attribute: x.Attribute!))
                                             .ToArray();

        var duplicateOpcodes = packetTypes.GroupBy(static x => x.Attribute.OpCode)
                                          .Where(static group => group.Count() > 1)
                                          .Select(static group => $"0x{group.Key:X2}")
                                          .ToArray();

        Assert.That(duplicateOpcodes, Is.Empty, "Duplicate opcode attributes found.");
        Assert.That(registry.RegisteredPackets.Count, Is.EqualTo(packetTypes.Length));

        foreach (var (packetType, attribute) in packetTypes)
        {
            var hasDescriptor = registry.TryGetDescriptor(attribute.OpCode, out var descriptor);
            var expectedLength = attribute.Sizing == PacketSizing.Fixed ? attribute.Length : -1;
            var expectedDescription = string.IsNullOrWhiteSpace(attribute.Description)
                ? packetType.Name.Replace("Packet", string.Empty, StringComparison.Ordinal)
                : attribute.Description!;

            Assert.Multiple(
                () =>
                {
                    Assert.That(hasDescriptor, Is.True, $"Missing descriptor for opcode 0x{attribute.OpCode:X2}");
                    Assert.That(descriptor.HandlerType, Is.EqualTo(packetType));
                    Assert.That(descriptor.Sizing, Is.EqualTo(attribute.Sizing));
                    Assert.That(descriptor.Length, Is.EqualTo(expectedLength));
                    Assert.That(descriptor.Description, Is.EqualTo(expectedDescription));
                }
            );
        }
    }
}
