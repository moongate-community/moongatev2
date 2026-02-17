using System.Text;
using Moongate.Core.Extensions.Strings;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Interfaces.FileLoaders;
using Moongate.UO.Data.Tiles;
using Moongate.UO.Data.Types;
using Serilog;

namespace Moongate.Server.FileLoaders;

public class TileDataLoader : IFileLoader

{
    private readonly ILogger _logger = Log.ForContext<TileDataLoader>();

    public async Task LoadAsync()
    {
        bool is64BitFlags;
        const int landLength = 0x4000;
        var filePath = UoFiles.GetFilePath("tiledata.mul");

        await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var bin = new BinaryReader(fs);

        int itemLength;

        if (fs.Length >= 3188736) // 7.0.9.0
        {
            is64BitFlags = true;
            itemLength = 0x10000;
        }
        else if (fs.Length >= 1644544) // 7.0.0.0
        {
            is64BitFlags = false;
            itemLength = 0x8000;
        }
        else
        {
            is64BitFlags = false;
            itemLength = 0x4000;
        }

        Span<byte> buffer = stackalloc byte[20];

        for (var i = 0; i < landLength; i++)
        {
            if (is64BitFlags ? i == 1 || i > 0 && (i & 0x1F) == 0 : (i & 0x1F) == 0)
            {
                bin.ReadInt32(); // header
            }

            var flags = (UOTileFlag)(is64BitFlags ? bin.ReadUInt64() : bin.ReadUInt32());
            bin.ReadInt16(); // skip 2 bytes -- textureID

            bin.Read(buffer);
            var terminator = buffer.IndexOfTerminator(1);
            var name = Encoding.ASCII.GetString(buffer[..(terminator < 0 ? buffer.Length : terminator)]);
            TileData.LandTable[i] = new(string.Intern(name), flags);
        }

        for (var i = 0; i < itemLength; i++)
        {
            if ((i & 0x1F) == 0)
            {
                bin.ReadInt32(); // header
            }

            var flags = (UOTileFlag)(is64BitFlags ? bin.ReadUInt64() : bin.ReadUInt32());
            int weight = bin.ReadByte();
            int quality = bin.ReadByte();
            int animation = bin.ReadUInt16();
            bin.ReadByte();
            int quantity = bin.ReadByte();
            bin.ReadInt32();
            bin.ReadByte();
            int value = bin.ReadByte();
            int height = bin.ReadByte();

            bin.Read(buffer);

            var terminator = buffer.IndexOfTerminator(1);
            var name = Encoding.ASCII.GetString(buffer[..(terminator < 0 ? buffer.Length : terminator)]);
            TileData.ItemTable[i] = new(
                string.Intern(name),
                flags,
                weight,
                quality,
                animation,
                quantity,
                value,
                height
            );
        }

        TileData.MaxLandValue = TileData.LandTable.Length - 1;
        TileData.MaxItemValue = TileData.ItemTable.Length - 1;

        _logger.Information(
            "TileData loaded: {LandCount} land entries, {ItemCount} item entries",
            TileData.LandTable.Length,
            TileData.ItemTable.Length
        );
    }
}
