using System.Runtime.CompilerServices;
using Moongate.UO.Data.Types;

namespace Moongate.UO.Data.Tiles;

public struct ItemData
{
    private byte _weight;
    private byte _quality;
    private ushort _animation;
    private byte _quantity;
    private byte _value;
    private byte _height;

    public ItemData(string name, UOTileFlag flags, int weight, int quality, int animation, int quantity, int value, int height)
    {
        Name = name;
        Flags = flags;
        _weight = (byte)weight;
        _quality = (byte)quality;
        _animation = (ushort)animation;
        _quantity = (byte)quantity;
        _value = (byte)value;
        _height = (byte)height;
    }

    public string Name { get; set; }

    public UOTileFlag Flags { get; set; }

    public int Weight
    {
        get => _weight;
        set => _weight = (byte)value;
    }

    public int Quality
    {
        get => _quality;
        set => _quality = (byte)value;
    }

    public int Animation
    {
        get => _animation;
        set => _animation = (ushort)value;
    }

    public int Quantity
    {
        get => _quantity;
        set => _quantity = (byte)value;
    }

    public int Value
    {
        get => _value;
        set => _value = (byte)value;
    }

    public int Height
    {
        get => _height;
        set => _height = (byte)value;
    }

    public int CalcHeight => Bridge ? _height / 2 : _height;

    public bool Door
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UOTileFlag.Door];
        set => this[UOTileFlag.Door] = value;
    }

    public bool Background
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UOTileFlag.Background];
        set => this[UOTileFlag.Background] = value;
    }

    public bool Bridge
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UOTileFlag.Bridge];
        set => this[UOTileFlag.Bridge] = value;
    }

    public bool Wall
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UOTileFlag.Wall];
        set => this[UOTileFlag.Wall] = value;
    }

    public bool Window
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UOTileFlag.Window];
        set => this[UOTileFlag.Window] = value;
    }

    public bool ImpassableSurface
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UOTileFlag.Impassable | UOTileFlag.Surface];
        set => this[UOTileFlag.Impassable | UOTileFlag.Surface] = value;
    }

    public bool Impassable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UOTileFlag.Impassable];
        set => this[UOTileFlag.Impassable] = value;
    }

    public bool Surface
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UOTileFlag.Surface];
        set => this[UOTileFlag.Surface] = value;
    }

    public bool Roof
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UOTileFlag.Roof];
        set => this[UOTileFlag.Roof] = value;
    }

    public bool LightSource
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UOTileFlag.LightSource];
        set => this[UOTileFlag.LightSource] = value;
    }

    public bool Wet
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UOTileFlag.Wet];
        set => this[UOTileFlag.Wet] = value;
    }

    public bool this[UOTileFlag flag]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (Flags & flag) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (value)
            {
                Flags |= flag;
            }
            else
            {
                Flags &= ~flag;
            }
        }
    }

    public override string ToString()
        => $" {Name} ({Flags})";
}
