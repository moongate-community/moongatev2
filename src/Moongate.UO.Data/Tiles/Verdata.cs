using Moongate.UO.Data.Files;

namespace Moongate.UO.Data.Tiles;

public sealed class Verdata
{
    public static Stream Stream { get; private set; }
    public static Entry5D[] Patches { get; private set; }

    private static string path;

    static Verdata()
    {
        Initialize();
    }

    public static void Initialize()
    {
        path = UoFiles.GetFilePath("verdata.mul");

        if (path == null)
        {
            Patches = [];
            Stream = Stream.Null;
        }
        else
        {
            using (Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var bin = new BinaryReader(Stream))
                {
                    Patches = new Entry5D[bin.ReadInt32()];

                    for (var i = 0; i < Patches.Length; ++i)
                    {
                        Patches[i].file = bin.ReadInt32();
                        Patches[i].index = bin.ReadInt32();
                        Patches[i].lookup = bin.ReadInt32();
                        Patches[i].length = bin.ReadInt32();
                        Patches[i].extra = bin.ReadInt32();
                    }
                }
            }

            Stream.Close();
        }
    }

    public static void Seek(int lookup)
    {
        if (Stream == null || !Stream.CanRead || !Stream.CanSeek)
        {
            if (path != null)
            {
                Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
        }

        Stream.Seek(lookup, SeekOrigin.Begin);
    }
}
