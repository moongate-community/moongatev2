using Moongate.Network.Packets.Base;
using Moongate.Network.Spans;

namespace Moongate.Tests.Network.Packets.Support;

public sealed class BaseGameNetworkTestPacket : BaseGameNetworkPacket
{
    public BaseGameNetworkTestPacket(byte opCode, int length)
        : base(opCode, length) { }

    public ushort ParsedValue { get; private set; }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        ParsedValue = reader.ReadUInt16();

        return true;
    }
}
