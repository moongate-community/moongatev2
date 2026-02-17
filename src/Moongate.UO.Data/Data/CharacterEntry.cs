using Moongate.Network.Spans;

namespace Moongate.UO.Data.Packets.Data;

public sealed class CharacterEntry
{
    public string Name { get; set; }

    public string Password { get; set; }

    public CharacterEntry(string name = "", string password = "")
    {
        Name = name;
        Password = password;
    }

    public static int Lenght => 60;

    public byte[] ToArray()
    {
        using var pr = new SpanWriter(stackalloc byte[60], true);

        pr.WriteAscii(Name, 30);
        pr.WriteAscii(Password, 30);

        return pr.Span.ToArray();
    }
}
