using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0x5D, PacketSizing.Fixed, Length = 73, Description = "Login Character")]
public class LoginCharacterPacket : BaseGameNetworkPacket
{
    public string CharacterName { get; set; } = string.Empty;

    public uint ClientIp { get; set; }

    public ClientFlags ClientFlags { get; set; } = ClientFlags.None;

    public int LoginCount { get; set; }

    public int Pattern1 { get; set; }

    public int Slot { get; set; }

    public ushort Unknown0 { get; set; }

    public int Unknown1 { get; set; }

    public byte[] Unknown2 { get; set; } = new byte[16];

    public LoginCharacterPacket()
        : base(0x5D, 73) { }

    public override void Write(ref SpanWriter writer)
    {
        var unknown2 = new byte[16];
        var sourceUnknown2 = Unknown2 ?? Array.Empty<byte>();
        sourceUnknown2.AsSpan(0, Math.Min(16, sourceUnknown2.Length)).CopyTo(unknown2);

        writer.Write(OpCode);
        writer.Write(Pattern1);
        writer.WriteAscii(CharacterName, 30);
        writer.Write(Unknown0);
        writer.Write((uint)ClientFlags);
        writer.Write(Unknown1);
        writer.Write(LoginCount);
        writer.Write(unknown2);
        writer.Write(Slot);
        writer.Write(ClientIp);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 72)
        {
            return false;
        }

        Pattern1 = reader.ReadInt32();
        CharacterName = reader.ReadAscii(30);
        Unknown0 = reader.ReadUInt16();
        ClientFlags = (ClientFlags)reader.ReadUInt32();
        Unknown1 = reader.ReadInt32();
        LoginCount = reader.ReadInt32();
        Unknown2 = reader.ReadBytes(16);
        Slot = reader.ReadInt32();
        ClientIp = reader.ReadUInt32();

        return reader.Remaining == 0;
    }
}
