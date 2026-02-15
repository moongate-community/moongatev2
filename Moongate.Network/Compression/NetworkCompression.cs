using System.Buffers;

namespace Moongate.Core.Network.Compression;

/// <summary>
/// Handles outgoing packet compression for the network.
/// </summary>
public static class NetworkCompression
{
    private const int CountIndex = 0;
    private const int ValueIndex = 1;

    /// <summary>
    /// UO packets may not exceed 64kb in length
    /// </summary>
    public const int BufferSize = 0x10000;

    /// <summary>
    /// Optimal compression ratio is 2 / 8;  worst compression ratio is 11 / 8
    /// </summary>
    private const int MinimalCodeLength = 2;

    private const int MaximalCodeLength = 11;

    /// <summary>
    /// Fixed overhead, in bits, per compression call
    /// </summary>
    private const int TerminalCodeLength = 4;

    /// <summary>
    /// If our input exceeds this length, we cannot possibly compress it within the buffer
    /// </summary>
    private const int DefiniteOverflow = (BufferSize * 8 - TerminalCodeLength) / MinimalCodeLength;

    private static readonly int[] _huffmanTable =
    {
        0x2, 0x000, 0x5, 0x01F, 0x6, 0x022, 0x7, 0x034, 0x7, 0x075, 0x6, 0x028, 0x6, 0x03B, 0x7, 0x032,
        0x8, 0x0E0, 0x8, 0x062, 0x7, 0x056, 0x8, 0x079, 0x9, 0x19D, 0x8, 0x097, 0x6, 0x02A, 0x7, 0x057,
        0x8, 0x071, 0x8, 0x05B, 0x9, 0x1CC, 0x8, 0x0A7, 0x7, 0x025, 0x7, 0x04F, 0x8, 0x066, 0x8, 0x07D,
        0x9, 0x191, 0x9, 0x1CE, 0x7, 0x03F, 0x9, 0x090, 0x8, 0x059, 0x8, 0x07B, 0x8, 0x091, 0x8, 0x0C6,
        0x6, 0x02D, 0x9, 0x186, 0x8, 0x06F, 0x9, 0x093, 0xA, 0x1CC, 0x8, 0x05A, 0xA, 0x1AE, 0xA, 0x1C0,
        0x9, 0x148, 0x9, 0x14A, 0x9, 0x082, 0xA, 0x19F, 0x9, 0x171, 0x9, 0x120, 0x9, 0x0E7, 0xA, 0x1F3,
        0x9, 0x14B, 0x9, 0x100, 0x9, 0x190, 0x6, 0x013, 0x9, 0x161, 0x9, 0x125, 0x9, 0x133, 0x9, 0x195,
        0x9, 0x173, 0x9, 0x1CA, 0x9, 0x086, 0x9, 0x1E9, 0x9, 0x0DB, 0x9, 0x1EC, 0x9, 0x08B, 0x9, 0x085,
        0x5, 0x00A, 0x8, 0x096, 0x8, 0x09C, 0x9, 0x1C3, 0x9, 0x19C, 0x9, 0x08F, 0x9, 0x18F, 0x9, 0x091,
        0x9, 0x087, 0x9, 0x0C6, 0x9, 0x177, 0x9, 0x089, 0x9, 0x0D6, 0x9, 0x08C, 0x9, 0x1EE, 0x9, 0x1EB,
        0x9, 0x084, 0x9, 0x164, 0x9, 0x175, 0x9, 0x1CD, 0x8, 0x05E, 0x9, 0x088, 0x9, 0x12B, 0x9, 0x172,
        0x9, 0x10A, 0x9, 0x08D, 0x9, 0x13A, 0x9, 0x11C, 0xA, 0x1E1, 0xA, 0x1E0, 0x9, 0x187, 0xA, 0x1DC,
        0xA, 0x1DF, 0x7, 0x074, 0x9, 0x19F, 0x8, 0x08D, 0x8, 0x0E4, 0x7, 0x079, 0x9, 0x0EA, 0x9, 0x0E1,
        0x8, 0x040, 0x7, 0x041, 0x9, 0x10B, 0x9, 0x0B0, 0x8, 0x06A, 0x8, 0x0C1, 0x7, 0x071, 0x7, 0x078,
        0x8, 0x0B1, 0x9, 0x14C, 0x7, 0x043, 0x8, 0x076, 0x7, 0x066, 0x7, 0x04D, 0x9, 0x08A, 0x6, 0x02F,
        0x8, 0x0C9, 0x9, 0x0CE, 0x9, 0x149, 0x9, 0x160, 0xA, 0x1BA, 0xA, 0x19E, 0xA, 0x39F, 0x9, 0x0E5,
        0x9, 0x194, 0x9, 0x184, 0x9, 0x126, 0x7, 0x030, 0x8, 0x06C, 0x9, 0x121, 0x9, 0x1E8, 0xA, 0x1C1,
        0xA, 0x11D, 0xA, 0x163, 0xA, 0x385, 0xA, 0x3DB, 0xA, 0x17D, 0xA, 0x106, 0xA, 0x397, 0xA, 0x24E,
        0x7, 0x02E, 0x8, 0x098, 0xA, 0x33C, 0xA, 0x32E, 0xA, 0x1E9, 0x9, 0x0BF, 0xA, 0x3DF, 0xA, 0x1DD,
        0xA, 0x32D, 0xA, 0x2ED, 0xA, 0x30B, 0xA, 0x107, 0xA, 0x2E8, 0xA, 0x3DE, 0xA, 0x125, 0xA, 0x1E8,
        0x9, 0x0E9, 0xA, 0x1CD, 0xA, 0x1B5, 0x9, 0x165, 0xA, 0x232, 0xA, 0x2E1, 0xB, 0x3AE, 0xB, 0x3C6,
        0xB, 0x3E2, 0xA, 0x205, 0xA, 0x29A, 0xA, 0x248, 0xA, 0x2CD, 0xA, 0x23B, 0xB, 0x3C5, 0xA, 0x251,
        0xA, 0x2E9, 0xA, 0x252, 0x9, 0x1EA, 0xB, 0x3A0, 0xB, 0x391, 0xA, 0x23C, 0xB, 0x392, 0xB, 0x3D5,
        0xA, 0x233, 0xA, 0x2CC, 0xB, 0x390, 0xA, 0x1BB, 0xB, 0x3A1, 0xB, 0x3C4, 0xA, 0x211, 0xA, 0x203,
        0x9, 0x12A, 0xA, 0x231, 0xB, 0x3E0, 0xA, 0x29B, 0xB, 0x3D7, 0xA, 0x202, 0xB, 0x3AD, 0xA, 0x213,
        0xA, 0x253, 0xA, 0x32C, 0xA, 0x23D, 0xA, 0x23F, 0xA, 0x32F, 0xA, 0x11C, 0xA, 0x384, 0xA, 0x31C,
        0xA, 0x17C, 0xA, 0x30A, 0xA, 0x2E0, 0xA, 0x276, 0xA, 0x250, 0xB, 0x3E3, 0xA, 0x396, 0xA, 0x18F,
        0xA, 0x204, 0xA, 0x206, 0xA, 0x230, 0xA, 0x265, 0xA, 0x212, 0xA, 0x23E, 0xB, 0x3AC, 0xB, 0x393,
        0xB, 0x3E1, 0xA, 0x1DE, 0xB, 0x3D6, 0xA, 0x31D, 0xB, 0x3E5, 0xB, 0x3E4, 0xA, 0x207, 0xB, 0x3C7,
        0xA, 0x277, 0xB, 0x3D4, 0x8, 0x0C0, 0xA, 0x162, 0xA, 0x3DA, 0xA, 0x124, 0xA, 0x1B4, 0xA, 0x264,
        0xA, 0x33D, 0xA, 0x1D1, 0xA, 0x1AF, 0xA, 0x39E, 0xA, 0x24F, 0xB, 0x373, 0xA, 0x249, 0xB, 0x372,
        0x9, 0x167, 0xA, 0x210, 0xA, 0x23A, 0xA, 0x1B8, 0xB, 0x3AF, 0xA, 0x18E, 0xA, 0x2EC, 0x7, 0x062,
        0x4, 0x00D
    };

    /// <summary>
    /// Tree node for huffman decompression
    /// </summary>
    private struct TreeNode
    {
        public bool IsLeaf;
        public int Value;
        public int NextPosition;
    }

    /// <summary>
    /// Calculates the maximum size needed for the compressed output buffer.
    /// This provides a conservative estimate based on the worst-case compression scenario.
    /// </summary>
    /// <param name="inputLength">Length of the input data to be compressed</param>
    /// <returns>Maximum size in bytes needed for the compressed output buffer, or 0 if input is too large to compress</returns>
    public static int CalculateMaxCompressedSize(int inputLength)
    {
        if (inputLength <= 0)
        {
            return 0;
        }

        if (inputLength > DefiniteOverflow)
        {
            return 0; // Input too large to compress
        }

        /// Worst case: each byte uses MaximalCodeLength bits
        var maxBitsNeeded = inputLength * MaximalCodeLength + TerminalCodeLength;

        /// Convert to bytes, rounding up for byte alignment
        var maxBytesNeeded = (maxBitsNeeded + 7) / 8;

        /// Ensure we don't exceed the buffer size limit
        return Math.Min(maxBytesNeeded, BufferSize);
    }

    /// <summary>
    /// Compresses input data using Huffman compression algorithm.
    /// </summary>
    /// <param name="input">Input data to compress</param>
    /// <param name="output">Output buffer for compressed data</param>
    /// <returns>Number of bytes written to output buffer, or 0 if compression failed</returns>
    public static int Compress(ReadOnlySpan<byte> input, Span<byte> output)
    {
        if (input.Length > DefiniteOverflow)
        {
            return 0;
        }

        var bitCount = 0;
        var bitValue = 0;
        var inputIdx = 0;
        var outputIdx = 0;

        while (inputIdx < input.Length)
        {
            var i = input[inputIdx++] << 1;
            bitCount += _huffmanTable[i];
            bitValue = (bitValue << _huffmanTable[i]) | _huffmanTable[i + 1];

            while (bitCount >= 8)
            {
                bitCount -= 8;

                if (output.Length < outputIdx + 1)
                {
                    return 0;
                }

                output[outputIdx++] = (byte)(bitValue >> bitCount);
            }
        }

        /// Terminal code
        bitCount += _huffmanTable[0x200];
        bitValue = (bitValue << _huffmanTable[0x200]) | _huffmanTable[0x201];

        /// Align on byte boundary
        if ((bitCount & 7) != 0)
        {
            bitValue <<= 8 - (bitCount & 7);
            bitCount += 8 - (bitCount & 7);
        }

        while (bitCount >= 8)
        {
            bitCount -= 8;

            if (output.Length < outputIdx + 1)
            {
                return 0;
            }

            output[outputIdx++] = (byte)(bitValue >> bitCount);
        }

        return outputIdx;
    }

    /// <summary>
    /// Compresses input data and returns the result as a new Memory
    /// <byte>
    /// .
    /// This method allocates a new buffer for the compressed output.
    /// </summary>
    /// <param name="input">Input data to compress</param>
    /// <returns>Compressed data as Memory<byte>, or empty if compression failed</returns>
    public static Memory<byte> CompressToMemory(ReadOnlyMemory<byte> input)
    {
        if (input.Length == 0)
        {
            return Memory<byte>.Empty;
        }

        var maxCompressedSize = CalculateMaxCompressedSize(input.Length);

        if (maxCompressedSize == 0)
        {
            return Memory<byte>.Empty; // Input too large or invalid
        }

        var outputBuffer = new byte[maxCompressedSize];
        var compressedLength = Compress(input.Span, outputBuffer);

        if (compressedLength == 0)
        {
            return Memory<byte>.Empty; // Compression failed
        }

        /// Return only the used portion of the buffer
        return new(outputBuffer, 0, compressedLength);
    }

    /// <summary>
    /// Decompresses Huffman-compressed data.
    /// </summary>
    /// <param name="input">Compressed input data</param>
    /// <param name="output">Output buffer for decompressed data</param>
    /// <returns>Number of bytes written to output buffer, or 0 if decompression failed</returns>
    public static int Decompress(ReadOnlySpan<byte> input, Span<byte> output)
    {
        if (input.Length == 0)
        {
            return 0;
        }

        var bitCount = 0;
        var bitValue = 0;
        var inputIdx = 0;
        var outputIdx = 0;
        var treePosition = 0;

        /// Build decompression tree from huffman table
        var tree = BuildDecompressionTree();

        while (inputIdx < input.Length && outputIdx < output.Length)
        {
            /// Read bits from input
            while (bitCount < 8 && inputIdx < input.Length)
            {
                bitValue = (bitValue << 8) | input[inputIdx++];
                bitCount += 8;
            }

            if (bitCount == 0)
            {
                break;
            }

            /// Process bits through huffman tree
            while (bitCount > 0 && outputIdx < output.Length)
            {
                bitCount--;
                var bit = (bitValue >> bitCount) & 1;

                /// Navigate through tree
                if (tree.TryGetValue((treePosition << 1) | bit, out var result))
                {
                    if (result.IsLeaf)
                    {
                        if (result.Value == 256) /// Terminal code
                        {
                            return outputIdx;
                        }

                        output[outputIdx++] = (byte)result.Value;
                        treePosition = 0; /// Reset to root
                    }
                    else
                    {
                        treePosition = result.NextPosition;
                    }
                }
                else
                {
                    /// Invalid path in tree
                    return 0;
                }
            }
        }

        return outputIdx;
    }

    /// <summary>
    /// Decompresses data and returns the result as a new Memory<byte>.
    /// </summary>
    /// <param name="input">Compressed input data</param>
    /// <param name="maxOutputSize">Maximum expected size of decompressed data</param>
    /// <returns>Decompressed data as Memory<byte>, or empty if decompression failed</returns>
    public static Memory<byte> DecompressToMemory(ReadOnlyMemory<byte> input, int maxOutputSize = BufferSize)
    {
        if (input.Length == 0)
        {
            return Memory<byte>.Empty;
        }

        var outputBuffer = new byte[maxOutputSize];
        var decompressedLength = Decompress(input.Span, outputBuffer);

        if (decompressedLength == 0)
        {
            return Memory<byte>.Empty;
        }

        /// Return only the used portion of the buffer
        return new(outputBuffer, 0, decompressedLength);
    }

    /// <summary>
    /// Checks if data appears to be compressed by looking for huffman patterns.
    /// This is a heuristic check and may not be 100% accurate.
    /// </summary>
    /// <param name="data">Data to check</param>
    /// <returns>True if data appears to be compressed</returns>
    public static bool IsCompressed(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
        {
            return false;
        }

        /// Simple heuristic: compressed data tends to have higher entropy
        /// and specific bit patterns. This is not foolproof.
        var uniqueBytes = 0;
        var seen = new bool[256];

        for (var i = 0; i < Math.Min(data.Length, 64); i++)
        {
            if (!seen[data[i]])
            {
                seen[data[i]] = true;
                uniqueBytes++;
            }
        }

        /// If we see a wide variety of byte values in a small sample,
        /// it's likely compressed data
        return uniqueBytes > data.Length / 4;
    }

    /// <summary>
    /// Main processing method compatible with ProcessReceive interface.
    /// Attempts to decompress data, with fallback handling.
    /// </summary>
    /// <param name="input">Input data to process</param>
    /// <param name="output">Processed output data</param>
    /// <returns>Tuple indicating if processing should halt and bytes consumed</returns>
    public static (bool halt, int consumedFromOrigin) ProcessReceive(
        ref ReadOnlyMemory<byte> input,
        out ReadOnlyMemory<byte> output
    )
    {
        output = input;

        if (input.Length == 0)
        {
            return (false, 0);
        }

        /// Try to detect if data is compressed
        if (!IsCompressed(input.Span))
        {
            /// Data doesn't appear compressed, return as-is
            output = input;

            return (false, input.Length);
        }

        /// Try decompression with a reasonable buffer size
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

        try
        {
            if (TryDecompress(input, buffer.AsSpan(0, BufferSize), out var decompressed))
            {
                output = decompressed;

                return (false, input.Length);
            }
            else
            {
                /// Decompression failed, return original data
                output = input;

                return (false, input.Length);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Advanced ProcessReceive with explicit compression flag.
    /// Use this when you have a header or flag indicating compression status.
    /// </summary>
    /// <param name="input">Input data to process</param>
    /// <param name="isCompressed">Flag indicating if data is compressed</param>
    /// <param name="output">Processed output data</param>
    /// <returns>Tuple indicating if processing should halt and bytes consumed</returns>
    public static (bool halt, int consumedFromOrigin) ProcessReceiveWithFlag(
        ref ReadOnlyMemory<byte> input,
        bool isCompressed,
        out ReadOnlyMemory<byte> output
    )
    {
        output = input;

        if (input.Length == 0)
        {
            return (false, 0);
        }

        if (!isCompressed)
        {
            /// Data is not compressed
            output = input;

            return (false, input.Length);
        }

        /// Data is compressed, attempt decompression
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

        try
        {
            if (TryDecompress(input, buffer.AsSpan(0, BufferSize), out var decompressed))
            {
                output = decompressed;

                return (false, input.Length);
            }
            else
            {
                /// Decompression failed - this is an error condition
                output = Memory<byte>.Empty;

                return (true, 0); /// Halt processing due to error
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Main processing method compatible with ProcessSend interface.
    /// Compresses data if beneficial, otherwise returns original data.
    /// </summary>
    /// <param name="input">Input data to process</param>
    /// <param name="output">Processed output data</param>
    public static void ProcessSend(ref ReadOnlyMemory<byte> input, out ReadOnlyMemory<byte> output)
    {
        /// Quick check if compression is worth attempting
        if (!ShouldCompress(input.Length))
        {
            output = input;

            return;
        }

        /// Try to compress using a temporary buffer
        var maxSize = CalculateMaxCompressedSize(input.Length);

        if (maxSize == 0)
        {
            output = input;

            return;
        }

        /// Use ArrayPool for better memory management in high-performance scenarios
        var buffer = ArrayPool<byte>.Shared.Rent(maxSize);

        try
        {
            if (TryCompress(input, buffer.AsSpan(0, maxSize), out var compressed))
            {
                output = compressed;
            }
            else
            {
                output = input;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Checks if the input data is worth compressing based on size thresholds.
    /// </summary>
    /// <param name="inputLength">Length of input data</param>
    /// <returns>True if compression should be attempted, false otherwise</returns>
    public static bool ShouldCompress(int inputLength)
    {
        /// Don't compress very small packets (compression overhead not worth it)
        const int minCompressionThreshold = 32;

        return inputLength >= minCompressionThreshold && inputLength <= DefiniteOverflow;
    }

    /// <summary>
    /// Attempts to compress input data in-place using a provided buffer.
    /// Returns true if compression was successful and beneficial.
    /// </summary>
    /// <param name="input">Input data to compress</param>
    /// <param name="buffer">Working buffer for compression (should be at least CalculateMaxCompressedSize bytes)</param>
    /// <param name="compressed">Output compressed data if successful</param>
    /// <returns>True if compression was successful and beneficial, false otherwise</returns>
    public static bool TryCompress(ReadOnlyMemory<byte> input, Span<byte> buffer, out Memory<byte> compressed)
    {
        compressed = Memory<byte>.Empty;

        if (input.Length == 0)
        {
            return false;
        }

        var compressedLength = Compress(input.Span, buffer);

        if (compressedLength == 0 || compressedLength >= input.Length)
        {
            /// Compression failed or wasn't beneficial
            return false;
        }

        /// Create a new array with exact size for the compressed data
        var result = new byte[compressedLength];
        buffer.Slice(0, compressedLength).CopyTo(result);
        compressed = result;

        return true;
    }

    /// <summary>
    /// Attempts to decompress data using a provided buffer.
    /// </summary>
    /// <param name="input">Compressed input data</param>
    /// <param name="buffer">Output buffer for decompressed data</param>
    /// <param name="decompressed">Decompressed data if successful</param>
    /// <returns>True if decompression was successful</returns>
    public static bool TryDecompress(ReadOnlyMemory<byte> input, Span<byte> buffer, out Memory<byte> decompressed)
    {
        decompressed = Memory<byte>.Empty;

        if (input.Length == 0)
        {
            return false;
        }

        var decompressedLength = Decompress(input.Span, buffer);

        if (decompressedLength == 0)
        {
            return false;
        }

        /// Create a new array with exact size for the decompressed data
        var result = new byte[decompressedLength];
        buffer.Slice(0, decompressedLength).CopyTo(result);
        decompressed = result;

        return true;
    }

    /// <summary>
    /// Builds the decompression tree from the huffman table
    /// </summary>
    private static Dictionary<int, TreeNode> BuildDecompressionTree()
    {
        var tree = new Dictionary<int, TreeNode>();
        var nextPosition = 1;

        /// Process each entry in huffman table (pairs of length, value)
        for (var i = 0; i < _huffmanTable.Length; i += 2)
        {
            var codeLength = _huffmanTable[i];
            var codeValue = _huffmanTable[i + 1];
            var byteValue = i / 2;

            if (codeLength == 0)
            {
                continue;
            }

            /// Build path through tree for this code
            var position = 0;

            for (var bit = codeLength - 1; bit >= 0; bit--)
            {
                var bitValue = (codeValue >> bit) & 1;
                var key = (position << 1) | bitValue;

                if (bit == 0) /// Leaf node
                {
                    tree[key] = new() { IsLeaf = true, Value = byteValue };
                }
                else /// Internal node
                {
                    if (!tree.TryGetValue(key, out var node) || node.IsLeaf)
                    {
                        tree[key] = new() { IsLeaf = false, NextPosition = nextPosition++ };
                    }

                    position = tree[key].NextPosition;
                }
            }
        }

        /// Add terminal code (256)
        tree[0x200] = new() { IsLeaf = true, Value = 256 };

        return tree;
    }
}
