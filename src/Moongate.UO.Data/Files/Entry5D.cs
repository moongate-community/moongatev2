namespace Moongate.UO.Data.Files;

public struct Entry5D
{
    public int file;
    public int index;
    public int lookup;
    public int length;
    public int extra;

    public override string ToString()
        => $"File: {file}, Index: {index}, Lookup: {lookup}, Length: {length}, Extra: {extra}";
}
