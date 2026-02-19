using Moongate.UO.Data.Geometry;

namespace Moongate.Tests.UO.Data.Geometry;

public class Point2DTests
{
    [Test]
    public void EqualityOperators_ShouldWorkAsExpected()
    {
        var a = new Point2D(3, 3);
        var b = new Point2D(3, 3);
        var c = new Point2D(3, 4);

        Assert.Multiple(
            () =>
            {
                Assert.That(a == b, Is.True);
                Assert.That(a != c, Is.True);
                Assert.That(a.Equals(b), Is.True);
            }
        );
    }

    [Test]
    public void Parse_WithInvalidInput_ShouldThrowFormatException()
    {
        Assert.That(() => Point2D.Parse("10,20"), Throws.TypeOf<FormatException>());
    }

    [Test]
    public void Parse_WithValidInput_ShouldReturnExpectedPoint()
    {
        var point = Point2D.Parse("(10, 20)");

        Assert.That(point.X, Is.EqualTo(10));
        Assert.That(point.Y, Is.EqualTo(20));
    }

    [Test]
    public void ToString_ShouldMatchExpectedFormat()
    {
        var point = new Point2D(4, 5);

        Assert.That(point.ToString(), Is.EqualTo("(4, 5)"));
    }

    [Test]
    public void TryParse_WithInvalidInput_ShouldReturnFalse()
    {
        var parsed = Point2D.TryParse("(x, y)", null, out var point);

        Assert.Multiple(
            () =>
            {
                Assert.That(parsed, Is.False);
                Assert.That(point, Is.EqualTo(default(Point2D)));
            }
        );
    }

    [Test]
    public void TryParse_WithValidInput_ShouldReturnTrueAndPoint()
    {
        var parsed = Point2D.TryParse("(7, 9)", null, out var point);

        Assert.Multiple(
            () =>
            {
                Assert.That(parsed, Is.True);
                Assert.That(point.X, Is.EqualTo(7));
                Assert.That(point.Y, Is.EqualTo(9));
            }
        );
    }
}
