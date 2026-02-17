using System.Text;
using Moongate.UO.Data.Context;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Interfaces.FileLoaders;
using Moongate.UO.Data.Localization;
using Moongate.UO.Data.Utils;
using Serilog;

namespace Moongate.Server.FileLoaders;

public class CliLocLoader : IFileLoader
{
    private static byte[] _buffer = new byte[1024];

    private readonly ILogger _logger = Log.ForContext<CliLocLoader>();

    public Task LoadAsync()
    {
        var cliLocFile = UoFiles.FindDataFile("cliloc.enu");

        var entries = ReadCliLocFile(cliLocFile, true);

        UOContext.LocalizedMessages = entries.ToDictionary(
            entry => entry.Number,
            entry => entry
        );

        _logger.Information("Loaded {Count} localized messages from {FilePath}", entries.Count, cliLocFile);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Reads and parses a cliloc.enu file returning a list of CliLocEntry objects
    /// </summary>
    /// <param name="filePath">Path to the cliloc.enu file</param>
    /// <returns>List of parsed cliloc entries</returns>
    public static List<StringEntry> ReadCliLocFile(string filePath, bool decompress)
    {
        var entries = new List<StringEntry>();

        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var buffer = new byte[fileStream.Length];
            _ = fileStream.Read(buffer, 0, buffer.Length);

            var clilocData = decompress
                                 ? MythicDecompress.Decompress(buffer)
                                 : buffer;

            using (var reader = new BinaryReader(new MemoryStream(clilocData)))
            {
                _ = reader.ReadInt32();
                _ = reader.ReadInt16();

                while (reader.BaseStream.Length != reader.BaseStream.Position)
                {
                    var number = reader.ReadInt32();
                    var flag = reader.ReadByte();
                    int length = reader.ReadInt16();

                    if (length > _buffer.Length)
                    {
                        _buffer = new byte[(length + 1023) & ~1023];
                    }

                    reader.Read(_buffer, 0, length);
                    var text = Encoding.UTF8.GetString(_buffer, 0, length);

                    var se = new StringEntry(number, text, flag);
                    entries.Add(se);
                }
            }
        }

        return entries;
    }
}
