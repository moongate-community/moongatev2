namespace Moongate.Core.Buffers;

internal sealed class STArrayPoolRentReturnStatus
{
    public string StackTrace { get; set; } = string.Empty;
    public bool IsRented { get; set; }
}
