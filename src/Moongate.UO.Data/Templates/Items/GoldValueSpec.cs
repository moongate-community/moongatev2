using System.Globalization;
using Moongate.Core.DiceNotation;
using ShaiRandom.Generators;

namespace Moongate.UO.Data.Templates.Items;

/// <summary>
/// Represents gold value configuration that can be a fixed number or a dice expression.
/// </summary>
public readonly record struct GoldValueSpec
{
    private GoldValueSpec(int fixedValue, string? diceExpression)
    {
        FixedValue = fixedValue;
        DiceExpression = diceExpression;
    }

    public int FixedValue { get; }

    public string? DiceExpression { get; }

    public bool IsDiceExpression => DiceExpression is not null;

    public static GoldValueSpec FromDiceExpression(string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);

        return new(0, expression);
    }

    public static GoldValueSpec FromValue(int value)
        => new(value, null);

    public int Resolve(IEnhancedRandom? rng = null)
    {
        if (!IsDiceExpression)
        {
            return FixedValue;
        }

        return Dice.Roll(DiceExpression!, rng);
    }

    public override string ToString()
        => IsDiceExpression
               ? $"dice({DiceExpression})"
               : FixedValue.ToString(CultureInfo.InvariantCulture);
}
