using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Interfaces;

public interface IGameNetworkPacket
{
    byte OpCode { get; }
    int Length { get; } // if -1 then variable length, otherwise fixed length


    bool TryParse(ReadOnlySpan<byte> data);

    void Write(ref SpanWriter writer);

}
