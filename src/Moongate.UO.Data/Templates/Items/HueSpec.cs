using Moongate.Core.Random;
using ShaiRandom.Generators;

namespace Moongate.UO.Data.Templates.Items;

/// <summary>
/// Represents a hue specification that can be either a fixed value or a runtime range.
/// </summary>
public readonly record struct HueSpec
{
    private HueSpec(int min, int max, bool isRange)
    {
        Min = min;
        Max = max;
        IsRange = isRange;
    }

    public int Min { get; }

    public int Max { get; }

    public bool IsRange { get; }

    public static HueSpec FromRange(int min, int max)
    {
        if (min > max)
        {
            throw new ArgumentOutOfRangeException(nameof(min), "min cannot be greater than max");
        }

        return new(min, max, true);
    }

    public static HueSpec FromValue(int value)
        => new(value, value, false);

    public int Resolve(IEnhancedRandom? rng = null)
    {
        if (!IsRange)
        {
            return Min;
        }

        rng ??= BuiltInRng.Generator;

        return rng.NextInt(Min, Max + 1);
    }

    public override string ToString()
        => IsRange ? $"hue({Min}:{Max})" : $"0x{Min:X4}";
}
