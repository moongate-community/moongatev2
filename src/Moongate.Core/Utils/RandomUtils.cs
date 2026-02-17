using System.Numerics;
using System.Runtime.CompilerServices;
using Moongate.Core.Random;

namespace Moongate.Core.Utils;

public static class RandomUtils
{
    // Optimized method for handling 50% random chances in succession up to a maximum
    public static int CoinFlips(int amount, int maximum)
    {
        var heads = 0;

        while (amount > 0)
        {
            // Range is 2^amount exclusively, maximum of 62 bits can be used
            var num = amount >= 62
                          ? (ulong)BuiltInRng.NextLong()
                          : (ulong)BuiltInRng.Next(1L << amount);

            heads += BitOperations.PopCount(num);

            if (heads >= maximum)
            {
                return maximum;
            }

            // 64 bits minus sign bit and exclusive maximum leaves 62 bits
            amount -= 62;
        }

        return heads;
    }

    public static int CoinFlips(int amount)
    {
        var heads = 0;

        while (amount > 0)
        {
            // Range is 2^amount exclusively, maximum of 62 bits can be used
            var num = amount >= 62
                          ? (ulong)BuiltInRng.NextLong()
                          : (ulong)BuiltInRng.Next(1L << amount);

            heads += BitOperations.PopCount(num);

            // 64 bits minus sign bit and exclusive maximum leaves 62 bits
            amount -= 62;
        }

        return heads;
    }

    public static int Dice(int amount, int sides, int bonus)
    {
        if (amount <= 0 || sides <= 0)
        {
            return 0;
        }

        int total;

        if (sides == 2)
        {
            total = CoinFlips(amount);
        }
        else
        {
            total = 0;

            for (var i = 0; i < amount; ++i)
            {
                total += BuiltInRng.Next(1, sides);
            }
        }

        return total + bonus;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Random(int from, int count)
        => BuiltInRng.Next(from, count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Random(int count)
        => count < 0 ? -BuiltInRng.Next(-count) : BuiltInRng.Next(count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Random(long from, long count)
        => BuiltInRng.Next(from, count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Random(long count)
        => count < 0 ? -BuiltInRng.Next(-count) : BuiltInRng.Next(count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RandomBytes(Span<byte> buffer)
        => BuiltInRng.NextBytes(buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double RandomDouble()
        => BuiltInRng.NextDouble();

    public static TEntity RandomList<TEntity>(params TEntity[] entities)
    {
        if (entities == null || entities.Length == 0)
        {
            return default;
        }

        var index = BuiltInRng.Next(entities.Length);

        return entities[index];
    }
}
