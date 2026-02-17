namespace Moongate.UO.Data.Version;

[Flags]
public enum ClientType
{
    None = 0x00,
    Classic = 0x01,
    UOTD = 0x02,
    KR = 0x04,
    SA = 0x08
}
