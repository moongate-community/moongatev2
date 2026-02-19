using Moongate.Network.Packets.Base;
using Moongate.Network.Spans;

namespace Moongate.Tests.Network.Packets.Support;

public sealed class BaseGameNetworkVariableLengthTestPacket : BaseGameNetworkPacket
{
    public BaseGameNetworkVariableLengthTestPacket(byte opCode)
        : base(opCode) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        _ = reader.ReadUInt16();

        return true;
    }
}
