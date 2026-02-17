namespace Moongate.UO.Data.Types;

[Flags]
public enum DirectionType : byte
{
    North = 0x0,
    NorthEast = 0x1,
    East = 0x2,
    SouthEast = 0x3,
    South = 0x4,
    SouthWest = 0x5,
    West = 0x6,
    NorthWest = 0x7,

    Running = 0x80

    /***
     *
     * 0x00 - North
       0x01 - Northeast
       0x02 - East
       0x03 - Southeast
       0x04 - South
       0x05 - Southwest
       0x06 - West
       0x07 - Northwest
     */
}
