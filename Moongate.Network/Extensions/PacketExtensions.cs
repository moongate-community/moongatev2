namespace Moongate.Network.Extensions;

public static class PacketExtensions
{
    public static string ToPacketString(this byte opCode)
        => "0x" + opCode.ToString("X2");
}
