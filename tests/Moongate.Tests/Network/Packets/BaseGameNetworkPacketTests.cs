using Moongate.Network.Packets.Base;
using Moongate.Network.Spans;

namespace Moongate.Tests.Network.Packets;

public class BaseGameNetworkPacketTests
{
    [Test]
    public void TryParse_WhenDataIsEmpty_ShouldReturnFalse()
    {
        var packet = new TestPacket(0xAA, 3);

        var result = packet.TryParse(ReadOnlySpan<byte>.Empty);

        Assert.That(result, Is.False);
    }

    [Test]
    public void TryParse_WhenOpCodeDoesNotMatch_ShouldReturnFalse()
    {
        var packet = new TestPacket(0xAA, 3);

        var result = packet.TryParse(new byte[] { 0xAB, 0x00, 0x05 });

        Assert.That(result, Is.False);
    }

    [Test]
    public void TryParse_WhenLengthIsFixedAndDifferent_ShouldReturnFalse()
    {
        var packet = new TestPacket(0xAA, 4);

        var result = packet.TryParse(new byte[] { 0xAA, 0x00, 0x05 });

        Assert.That(result, Is.False);
    }

    [Test]
    public void TryParse_WhenPayloadIsValid_ShouldReturnTrueAndParsePayload()
    {
        var packet = new TestPacket(0xAA, 3);

        var result = packet.TryParse(new byte[] { 0xAA, 0x01, 0xF4 });

        Assert.That(result, Is.True);
        Assert.That(packet.ParsedValue, Is.EqualTo(500));
    }

    [Test]
    public void TryParse_WhenPayloadIsIncomplete_ShouldReturnFalse()
    {
        var packet = new VariableLengthTestPacket(0xAA);

        var result = packet.TryParse(new byte[] { 0xAA, 0x01 });

        Assert.That(result, Is.False);
    }

    [Test]
    public void Write_WhenNotOverridden_ShouldThrowNotImplementedException()
    {
        var packet = new TestPacket(0xAA, 3);
        Span<byte> buffer = stackalloc byte[8];
        var writer = new SpanWriter(buffer);
        var didThrow = false;
        try
        {
            packet.Write(ref writer);
        }
        catch (NotImplementedException)
        {
            didThrow = true;
        }

        Assert.That(didThrow, Is.True);
    }

    private sealed class TestPacket(byte opCode, int length) : BaseGameNetworkPacket(opCode, length)
    {
        public ushort ParsedValue { get; private set; }

        protected override bool ParsePayload(ref SpanReader reader)
        {
            ParsedValue = reader.ReadUInt16();
            return true;
        }
    }

    private sealed class VariableLengthTestPacket(byte opCode) : BaseGameNetworkPacket(opCode, -1)
    {
        protected override bool ParsePayload(ref SpanReader reader)
        {
            _ = reader.ReadUInt16();
            return true;
        }
    }
}
