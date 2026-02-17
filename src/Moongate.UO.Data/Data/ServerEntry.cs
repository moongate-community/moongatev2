namespace Moongate.UO.Data.Packets.Data;

/// <summary>
/// Server entry for ServerListingPacket (0x5E)
/// </summary>
public class ServerEntry
{
    public byte Index { get; set; }
    public string ServerName { get; set; }

    public ServerEntry() { }

    public ServerEntry(byte index, string serverName)
    {
        Index = index;
        ServerName = serverName;
    }
}
