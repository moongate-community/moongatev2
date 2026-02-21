namespace Moongate.Network.Packets.Incoming.Books;

public class BookPageEntry
{
    public ushort PageNumber { get; set; }

    public ushort LineCount { get; set; }

    public List<string> Lines { get; }

    public bool IsPageRequest => LineCount == ushort.MaxValue;

    public BookPageEntry()
        => Lines = new List<string>();
}
