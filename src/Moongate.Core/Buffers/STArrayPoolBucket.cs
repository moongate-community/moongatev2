namespace Moongate.Core.Buffers;

internal struct STArrayPoolBucket<T>
{
    public T[]? Array;
    public long Ticks;

    public STArrayPoolBucket(T[] array)
    {
        Array = array;
        Ticks = 0;
    }
}
