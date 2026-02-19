using System.Net;
using Moongate.Core.Extensions.Network;
using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Outgoing.Login;

[PacketHandler(0x8C, PacketSizing.Fixed, Length = 11, Description = "Connect To Game Server")]
public class ServerRedirectPacket : BaseGameNetworkPacket
{
    public IPAddress IPAddress { get; set; }
    public int Port { get; set; }

    public uint SessionKey { get; set; }

    public ServerRedirectPacket()
        : base(0x8C, 11) { }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.WriteLE(IPAddress.ToRawAddress());
        writer.Write((ushort)Port);
        writer.Write(SessionKey);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
