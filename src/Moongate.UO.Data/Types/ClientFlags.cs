namespace Moongate.UO.Data.Types;

[Flags]
public enum ClientFlags
{
    None = 0x00000000,
    Felucca = 0x00000001,
    Trammel = 0x00000002,
    Ilshenar = 0x00000004,
    Malas = 0x00000008,
    Tokuno = 0x00000010,
    TerMur = 0x00000020,
    KR = 0x00000040,
    Unk2 = 0x00000080,
    UOTD = 0x00000100
}
