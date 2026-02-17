namespace Moongate.Network.Exceptions.Buffers;

/// <summary>
/// Exception thrown when an operation requires at least one element in the circular buffer.
/// </summary>
public sealed class CircularBufferEmptyException(string? message = null) : InvalidOperationException(
    message ?? "Circular buffer is empty."
);
