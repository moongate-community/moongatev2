using System.Runtime.InteropServices;

namespace Moongate.UO.Data.Utils;

public static class MythicDecompress
{
    public static byte[] Decompress(byte[] buffer)
    {
        byte[] output;

        using (var reader = new BinaryReader(new MemoryStream(buffer)))
        {
            var header = reader.ReadUInt32();
            var dataLength = header ^ 0x8E2C9A3D; // Must be equal to output length, error otherwise

            // MoveToFront decoding
            var list = reader.ReadBytes((int)(reader.BaseStream.Length - 4));
            output = InternalDecompress(MoveToFrontCoding.Decode(list));
        }

        return output;
    }

    public static byte[] Detransform(byte[] buffer)
        => InternalDecompress(MoveToFrontCoding.Decode(buffer));

    public static byte[] InternalCompress(Span<byte> input)
    {
        Span<byte> symbolTable = stackalloc byte[256];
        Span<byte> frequency = stackalloc byte[256];
        Span<int> partialInput = stackalloc int[256 * 3];

        // counting frequencies
        for (var i = 0; i < input.Length; ++i)
        {
            partialInput[input[i]]++;
        }

        Frequency(partialInput, frequency);

        var nonZeroCount = 0;

        for (var i = 0; i < 256; i++)
        {
            if (partialInput[i] != 0)
            {
                nonZeroCount++;
            }
        }

        var output = new byte[input.Length + nonZeroCount + 1024];

        for (int i = 0,
                 m = 0;
             i < nonZeroCount;
             ++i)
        {
            var freqIndex = frequency[i];
            partialInput[freqIndex + 256] = m + 1;
            m += partialInput[freqIndex];
            partialInput[freqIndex + 512] = m;
        }

        for (var i = 0; i < 256; ++i)
        {
            var bytes = BitConverter.GetBytes(partialInput[i]);
            output[i * 4] = bytes[0];
            output[i * 4 + 1] = bytes[1];
            output[i * 4 + 2] = bytes[2];
            output[i * 4 + 3] = bytes[3];
        }

        var count = input.Length - 1;

        var addedSymbols = new List<byte>(256); // keeping track for added symbols

        do
        {
            var val = input[count];

            ref var firstValRef = ref partialInput[val + 512];
            var outputAddress = firstValRef + 1024;

            // first add, just put it in symbolTable from the left and assign 0 idx
            if (!addedSymbols.Contains(val))
            {
                ShiftRight(symbolTable, addedSymbols.Count);
                symbolTable[0] = val;
                addedSymbols.Add(val);
                output[outputAddress] = 0;
            }

            // we're already have symbol in table, so getting it idx and putting it in output stream
            else if (firstValRef >= partialInput[val + 256])
            {
                var idx = GetIdx(symbolTable, val, addedSymbols.Count);
                ShiftRight(symbolTable, idx);
                symbolTable[0] = val;
                output[outputAddress] = idx;
            }

            firstValRef--;

            count--;
        } while (count >= 0);

        for (int i = 0,
                 m = 0;
             i < nonZeroCount;
             ++i)
        {
            var freqIndex = frequency[i];
            output[m + 1024] = GetIdx(symbolTable, freqIndex, nonZeroCount);
            m += partialInput[freqIndex];
        }

        return output;
    }

    public static byte[] InternalDecompress(Span<byte> input)
    {
        try
        {
            Span<byte> symbolTable = stackalloc byte[256];
            Span<byte> frequency = stackalloc byte[256];
            Span<int> partialInput = stackalloc int[256 * 3];

            partialInput.Clear();

            for (var i = 0; i < 256; i++)
            {
                symbolTable[i] = (byte)i;
            }

            input.Slice(0, 1024).CopyTo(MemoryMarshal.AsBytes(partialInput));

            var sum = 0;

            for (var i = 0; i < 256; i++)
            {
                sum += partialInput[i];
            }

            if (sum == 0)
            {
                return Array.Empty<byte>();
            }

            var output = new byte[sum];
            var count = 0;
            var nonZeroCount = 0;

            for (var i = 0; i < 256; i++)
            {
                if (partialInput[i] != 0)
                {
                    nonZeroCount++;
                }
            }

            Frequency(partialInput, frequency);

            for (int i = 0,
                     m = 0;
                 i < nonZeroCount;
                 ++i)
            {
                var freq = frequency[i];
                symbolTable[input[m + 1024]] = freq;
                partialInput[freq + 256] = m + 1;

                // TODO: check how safe is updating m counter inside a loop
                m += partialInput[freq];
                partialInput[freq + 512] = m;
            }

            var val = symbolTable[0];

            // TODO: expression is always false?
            if (sum == 0)
            {
                return output;
            }

            do
            {
                ref var firstValRef = ref partialInput[val + 256];
                output[count] = val;

                if (firstValRef < partialInput[val + 512])
                {
                    var idx = input[firstValRef + 1024];
                    firstValRef++;

                    if (idx != 0)
                    {
                        ShiftLeft(symbolTable, idx);

                        symbolTable[idx] = val;
                        val = symbolTable[0];
                    }
                }
                else if (nonZeroCount-- > 0)
                {
                    ShiftLeft(symbolTable, nonZeroCount);

                    val = symbolTable[0];
                }

                count++;
            } while (count < sum);

            return output;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during decompression: {ex.Message}");

            throw;
        }
    }

    public static byte[] Transform(byte[] buffer)
        => MoveToFrontCoding.Encode(InternalCompress(buffer));

    //
    // Looking for max value in freq table, adding it index to output and erasing to find next max value except last one
    //
    private static void Frequency(Span<int> input, Span<byte> output)
    {
        Span<int> tmp = stackalloc int[256];
        input.Slice(0, tmp.Length).CopyTo(tmp);

        for (var i = 0; i < 256; i++)
        {
            uint value = 0;
            byte index = 0;

            for (var j = 0; j < 256; j++)
            {
                if (tmp[j] > value)
                {
                    index = (byte)j;
                    value = (uint)tmp[j];
                }
            }

            if (value == 0)
            {
                break;
            }

            output[i] = index;
            tmp[index] = 0;
        }
    }

    private static byte GetIdx(Span<byte> input, byte val, int nonZeroCount)
    {
        for (byte i = 0; i < input.Length && i < nonZeroCount; ++i)
        {
            if (input[i] == val)
            {
                return i;
            }
        }

        return 0;
    }

    // element - this is index of element until which we re shifting our array to the left, destroying zero element
    private static void ShiftLeft(Span<byte> input, int element)
    {
        for (var i = 0; i < element; ++i)
        {
            input[i] = input[i + 1];
        }
    }

    private static void ShiftRight(Span<byte> input, int element)
    {
        for (var i = element; i >= 1; --i)
        {
            input[i] = input[i - 1];
        }
    }
}
