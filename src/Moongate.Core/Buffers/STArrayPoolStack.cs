using System.Runtime.CompilerServices;
using Moongate.Core.Buffers.Types;

namespace Moongate.Core.Buffers;

internal sealed class STArrayPoolStack<T>
{
    private readonly T[]?[] _arrays;
    private int _count;
    private long _ticks;

    public STArrayPoolStack(int stackArraySize)
    {
        _arrays = new T[]?[stackArraySize];
    }

    public void Trim(long now, STArrayPoolMemoryPressureType pressure, int bucketSize)
    {
        if (_count == 0)
        {
            return;
        }

        var threshold = pressure == STArrayPoolMemoryPressureType.High ? 10000 : 60000;

        if (_ticks == 0)
        {
            _ticks = now;
            return;
        }

        if (now - _ticks <= threshold)
        {
            return;
        }

        var trimCount = 1;

        switch (pressure)
        {
            case STArrayPoolMemoryPressureType.Medium:
                trimCount = 2;
                break;
            case STArrayPoolMemoryPressureType.High:
                if (bucketSize > 16384)
                {
                    trimCount++;
                }

                var size = Unsafe.SizeOf<T>();

                if (size > 32)
                {
                    trimCount += 2;
                }
                else if (size > 16)
                {
                    trimCount++;
                }

                break;
        }

        while (_count > 0 && trimCount-- > 0)
        {
            _arrays[--_count] = null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[]? TryPop()
    {
        var arrays = _arrays;
        var count = _count - 1;

        if ((uint)count < (uint)arrays.Length)
        {
            var arr = arrays[count];
            arrays[count] = null;
            _count = count;

            return arr;
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPush(T[] array)
    {
        var arrays = _arrays;
        var count = _count;

        if ((uint)count < (uint)arrays.Length)
        {
            arrays[count] = array;
            _count = count + 1;

            return true;
        }

        return false;
    }
}
