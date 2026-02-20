using Moongate.Core.DiceNotation;
using Moongate.Core.DiceNotation.Exceptions;

namespace Moongate.Tests.Core;

public class DiceNotationTests
{
    [Test]
    public void Parse_WhenExpressionIsValid_ShouldReturnExpression()
    {
        var expression = Dice.Parse("2d6+3");

        Assert.That(expression, Is.Not.Null);
    }

    [Test]
    public void MinRollAndMaxRoll_ForSimpleDiceExpression_ShouldMatchExpectedBounds()
    {
        var expression = Dice.Parse("2d6+3");

        Assert.Multiple(() =>
        {
            Assert.That(expression.MinRoll(), Is.EqualTo(5));
            Assert.That(expression.MaxRoll(), Is.EqualTo(15));
        });
    }

    [Test]
    public void Roll_ForSimpleDiceExpression_ShouldAlwaysStayWithinBounds()
    {
        const int min = 5;
        const int max = 15;

        for (var i = 0; i < 100; i++)
        {
            var roll = Dice.Roll("2d6+3");
            Assert.That(roll, Is.InRange(min, max));
        }
    }

    [Test]
    public void Roll_WhenMultiplicityIsNegative_ShouldThrowInvalidMultiplicityException()
    {
        Assert.That(() => Dice.Roll("-1d6"), Throws.TypeOf<InvalidMultiplicityException>());
    }

    [Test]
    public void Roll_WhenDieHasZeroSides_ShouldThrowImpossibleDieException()
    {
        Assert.That(() => Dice.Roll("1d0"), Throws.TypeOf<ImpossibleDieException>());
    }

    [Test]
    public void Roll_WhenKeepValueIsInvalid_ShouldThrowInvalidChooseException()
    {
        Assert.That(() => Dice.Roll("2d6k3"), Throws.TypeOf<InvalidChooseException>());
    }

    [Test]
    public void Parse_WhenKeepIsNotAppliedToDice_ShouldThrowInvalidSyntaxException()
    {
        Assert.That(() => Dice.Parse("1k1"), Throws.TypeOf<InvalidSyntaxException>());
    }
}
