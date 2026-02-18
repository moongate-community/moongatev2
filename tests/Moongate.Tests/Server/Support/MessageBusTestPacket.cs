using Moongate.Network.Packets.Interfaces;
using Moongate.Network.Spans;

namespace Moongate.Tests.Server.Support;

public sealed class MessageBusTestPacket : IGameNetworkPacket
{
    public MessageBusTestPacket(byte opCode)
        => OpCode = opCode;

    public byte OpCode { get; }
    public int Length => 1;

    public bool TryParse(ReadOnlySpan<byte> data)
        => true;

    public void Write(ref SpanWriter writer) { }
}
