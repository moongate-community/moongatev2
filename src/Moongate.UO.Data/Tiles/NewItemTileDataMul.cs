using System.Runtime.InteropServices;

namespace Moongate.UO.Data.Tiles;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct NewItemTileDataMul
{
    public int flags;
    public int unk1;
    public byte weight;
    public byte quality;
    public short miscdata;
    public byte unk2;
    public byte quantity;
    public short anim;
    public byte unk3;
    public byte hue;
    public byte stackingoffset;
    public byte value;
    public byte height;
    public fixed byte name[20];
}
