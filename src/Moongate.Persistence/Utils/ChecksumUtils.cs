namespace Moongate.Persistence.Utils;

/// <summary>
/// Computes checksums used to validate journal records.
/// </summary>
internal static class ChecksumUtils
{
    public static uint Compute(byte[] data)
    {
        const uint fnvPrime = 16777619;
        var hash = 2166136261u;

        for (var i = 0; i < data.Length; i++)
        {
            hash ^= data[i];
            hash *= fnvPrime;
        }

        return hash;
    }
}
