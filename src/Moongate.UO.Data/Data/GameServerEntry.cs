using System.Net;
using Moongate.Core.Extensions.Network;
using Moongate.Network.Spans;

namespace Moongate.UO.Data.Packets.Data;

public class GameServerEntry
{
    public int Index { get; set; }
    public string ServerName { get; set; }
    public IPAddress IpAddress { get; set; }

    public ReadOnlyMemory<byte> Write()
    {
        using var writer = new SpanWriter(1, true);

        writer.Write((short)Index);
        writer.WriteAscii(ServerName, 32);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write(IpAddress.ToRawAddress());

        return writer.ToArray().AsMemory();
    }
}
