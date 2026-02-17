using System.Buffers.Binary;
using Moongate.UO.Data.Context;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Interfaces.FileLoaders;
using Serilog;

namespace Moongate.Server.FileLoaders;

public class ClientVersionLoader : IFileLoader
{
    private readonly ILogger _logger = Log.ForContext<ClientVersionLoader>();

    public Task LoadAsync()
    {
        var path = UoFiles.FindDataFile("client.exe");

        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var buffer = GC.AllocateUninitializedArray<byte>((int)fs.Length, true);
        _ = fs.Read(buffer);

        // VS_VERSION_INFO (unicode)
        Span<byte> vsVersionInfo =
        [
            0x56, 0x00, 0x53, 0x00, 0x5F, 0x00, 0x56, 0x00,
            0x45, 0x00, 0x52, 0x00, 0x53, 0x00, 0x49, 0x00,
            0x4F, 0x00, 0x4E, 0x00, 0x5F, 0x00, 0x49, 0x00,
            0x4E, 0x00, 0x46, 0x00, 0x4F, 0x00
        ];

        var versionIndex = buffer.AsSpan().IndexOf(vsVersionInfo);

        if (versionIndex > -1)
        {
            var offset = versionIndex + 42; // 30 + 12

            var minorPart = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(offset));
            var majorPart = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(offset + 2));
            var privatePart = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(offset + 4));
            var buildPart = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(offset + 6));

            UOContext.ServerClientVersion = new(majorPart, minorPart, buildPart, privatePart);

            _logger.Information("Client version loaded: {Version}", UOContext.ServerClientVersion);
        }

        return Task.CompletedTask;
    }
}
