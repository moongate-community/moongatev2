using Moongate.Network.Packets.Incoming.Interaction;
using Moongate.Network.Packets.Incoming.Login;
using Moongate.Network.Packets.Incoming.Movement;
using Moongate.Network.Packets.Incoming.Speech;
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
            }
        );
    }

    [Test]
    public void Register_WhenOpcodeAlreadyRegistered_ShouldThrow()
    {
        var registry = new PacketRegistry();
        registry.RegisterFixed<MoveRequestPacket>(0x02, 7);

        Assert.That(() => registry.RegisterFixed<DoubleClickPacket>(0x02, 5), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void RegisterFixed_ShouldStoreDescriptorAndCreatePacket()
    {
        var registry = new PacketRegistry();

        registry.RegisterFixed<MoveRequestPacket>(0x02, 7);

        var hasDescriptor = registry.TryGetDescriptor(0x02, out var descriptor);
        var hasPacket = registry.TryCreatePacket(0x02, out var packet);

        Assert.That(hasDescriptor, Is.True);
        Assert.That(descriptor.OpCode, Is.EqualTo(0x02));
        Assert.That(descriptor.Sizing, Is.EqualTo(PacketSizing.Fixed));
        Assert.That(descriptor.Length, Is.EqualTo(7));
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

        registry.RegisterVariable<UnicodeSpeechPacket>(0xAD);

        var hasDescriptor = registry.TryGetDescriptor(0xAD, out var descriptor);

        Assert.That(hasDescriptor, Is.True);
        Assert.That(descriptor.Sizing, Is.EqualTo(PacketSizing.Variable));
        Assert.That(descriptor.Length, Is.EqualTo(-1));
    }
}
