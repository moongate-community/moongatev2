using Moongate.Network.Packets.Interfaces;
using Moongate.Network.Spans;

namespace Moongate.Tests.Server.Support;

public sealed class GameLoopTestPacket : IGameNetworkPacket
{
    public GameLoopTestPacket(byte opCode, int sequence)
    {
        OpCode = opCode;
        Sequence = sequence;
    }

    public int Sequence { get; }
    public byte OpCode { get; }
    public int Length => 1;

    public bool TryParse(ReadOnlySpan<byte> data)
        => true;

    public void Write(ref SpanWriter writer) { }
}
