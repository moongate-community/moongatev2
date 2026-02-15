namespace Moongate.Network.Exceptions.Buffers;

/// <summary>
/// Exception thrown when an index is outside the valid circular buffer bounds.
/// </summary>
public sealed class CircularBufferIndexOutOfRangeException(string paramName, int index, int size)
    : ArgumentOutOfRangeException(paramName, index, $"Cannot access index {index}. Buffer size is {size}.");
