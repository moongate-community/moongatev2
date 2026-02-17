/*************************************************************************
 * ModernUO                                                              *
 * Copyright 2019-2023 - ModernUO Development Team                       *
 * Email: hi@modernuo.com                                                *
 * File: TileData.cs                                                     *
 *                                                                       *
 * This program is free software: you can redistribute it and/or modify  *
 * it under the terms of the GNU General Public License as published by  *
 * the Free Software Foundation, either version 3 of the License, or     *
 * (at your option) any later version.                                   *
 *                                                                       *
 * You should have received a copy of the GNU General Public License     *
 * along with this program.  If not, see <http://www.gnu.org/licenses/>. *
 *************************************************************************/

using ZLinq;

namespace Moongate.UO.Data.Tiles;

public static class TileData
{
    public static LandData[] LandTable { get; } = new LandData[0x4000];
    public static ItemData[] ItemTable { get; } = new ItemData[0x10000];
    public static int MaxLandValue { get; set; }
    public static int MaxItemValue { get; set; }

    public static ItemData ItemDataByName(string name)
        => ItemTable.AsValueEnumerable()
                    .FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

    // public static unsafe void Configure()
    // {
    //     var filePath = UoFiles.GetFilePath("tiledata.mul");
    //
    //     using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    //     using var bin = new BinaryReader(fs);
    //
    //     bool is64BitFlags;
    //     const int landLength = 0x4000;
    //     int itemLength;
    //
    //     if (fs.Length >= 3188736) // 7.0.9.0
    //     {
    //         is64BitFlags = true;
    //         itemLength = 0x10000;
    //     }
    //     else if (fs.Length >= 1644544) // 7.0.0.0
    //     {
    //         is64BitFlags = false;
    //         itemLength = 0x8000;
    //     }
    //     else
    //     {
    //         is64BitFlags = false;
    //         itemLength = 0x4000;
    //     }
    //
    //     Span<byte> buffer = stackalloc byte[20];
    //
    //     for (var i = 0; i < landLength; i++)
    //     {
    //         if (is64BitFlags ? i == 1 || i > 0 && (i & 0x1F) == 0 : (i & 0x1F) == 0)
    //         {
    //             bin.ReadInt32(); // header
    //         }
    //
    //         var flags = (TileFlag)(is64BitFlags ? bin.ReadUInt64() : bin.ReadUInt32());
    //         bin.ReadInt16(); // skip 2 bytes -- textureID
    //
    //         bin.Read(buffer);
    //         var terminator = buffer.IndexOfTerminator(1);
    //         var name = Encoding.ASCII.GetString(buffer[..(terminator < 0 ? buffer.Length : terminator)]);
    //         LandTable[i] = new LandData(name.Intern(), flags);
    //     }
    //
    //     for (var i = 0; i < itemLength; i++)
    //     {
    //         if ((i & 0x1F) == 0)
    //         {
    //             bin.ReadInt32(); // header
    //         }
    //
    //         var flags = (TileFlag)(is64BitFlags ? bin.ReadUInt64() : bin.ReadUInt32());
    //         int weight = bin.ReadByte();
    //         int quality = bin.ReadByte();
    //         int animation = bin.ReadUInt16();
    //         bin.ReadByte();
    //         int quantity = bin.ReadByte();
    //         bin.ReadInt32();
    //         bin.ReadByte();
    //         int value = bin.ReadByte();
    //         int height = bin.ReadByte();
    //
    //         bin.Read(buffer);
    //
    //         var terminator = buffer.IndexOfTerminator(1);
    //         var name = Encoding.ASCII.GetString(buffer[..(terminator < 0 ? buffer.Length : terminator)]);
    //         ItemTable[i] = new ItemData(
    //             name.Intern(),
    //             flags,
    //             weight,
    //             quality,
    //             animation,
    //             quantity,
    //             value,
    //             height
    //         );
    //     }
    //
    //     MaxLandValue = LandTable.Length - 1;
    //     MaxItemValue = ItemTable.Length - 1;
    // }
}
