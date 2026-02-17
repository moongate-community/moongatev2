using System.Buffers.Binary;
using System.Net;

namespace Moongate.Core.Extensions.Network;

public static class IpAddressExtensions
{
    public static uint ToRawAddress(this IPEndPoint endPoint)
    {
        Span<byte> integer = stackalloc byte[4];
        endPoint.Address.MapToIPv4().TryWriteBytes(integer, out var bytesWritten);

        if (bytesWritten != 4)
        {
            throw new InvalidOperationException("IP Address could not be serialized to an integer");
        }

        return BinaryPrimitives.ReadUInt32LittleEndian(integer);
    }

    public static uint ToRawAddress(this IPAddress ipAddress)
    {
        Span<byte> integer = stackalloc byte[4];
        ipAddress.MapToIPv4().TryWriteBytes(integer, out var bytesWritten);

        if (bytesWritten != 4)
        {
            throw new InvalidOperationException("IP Address could not be serialized to an integer");
        }

        var ip = BinaryPrimitives.ReadUInt32LittleEndian(integer);

        return ip;
    }
}
