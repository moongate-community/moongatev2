using Moongate.UO.Data.Geometry;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.UO.Data.Geometry;

public class Point3DTests
{
    [Test]
    public void DirectionConversion_ShouldMapToExpectedOffset()
    {
        Point3D eastOffset = DirectionType.East;
        var moved = new Point3D(10, 10, 0).Move(DirectionType.East);

        Assert.Multiple(
            () =>
            {
                Assert.That(eastOffset, Is.EqualTo(new Point3D(1, 0, 0)));
                Assert.That(moved, Is.EqualTo(new Point3D(11, 10, 0)));
            }
        );
    }

    [Test]
    public void GetDirectionTo_ShouldReturnExpectedDirection()
    {
        var from = new Point3D(10, 10, 0);
        var to = new Point3D(11, 9, 0);

        Assert.That(from.GetDirectionTo(to), Is.EqualTo(DirectionType.NorthEast));
    }

    [Test]
    public void GetDistance_ShouldUse2DDistance()
    {
        var origin = new Point3D(0, 0, 0);
        var target = new Point3D(3, 4, 99);

        Assert.That(origin.GetDistance(target), Is.EqualTo(5d).Within(0.0001));
    }

    [Test]
    public void GetDistance3D_ShouldIncludeZAxis()
    {
        var origin = new Point3D(0, 0, 0);
        var target = new Point3D(2, 3, 6);

        Assert.That(origin.GetDistance3D(target), Is.EqualTo(7d).Within(0.0001));
    }

    [Test]
    public void InRangeAndInRange3D_ShouldRespectExpectedRules()
    {
        var origin = new Point3D(0, 0, 0);
        var sameXYDifferentZ = new Point3D(0, 0, 50);

        Assert.Multiple(
            () =>
            {
                Assert.That(origin.InRange(sameXYDifferentZ, 0), Is.True);
                Assert.That(origin.InRange3D(sameXYDifferentZ, 0), Is.False);
            }
        );
    }

    [Test]
    public void Parse_WithValidInput_ShouldReturnExpectedPoint()
    {
        var point = Point3D.Parse("(10, 20, -5)");

        Assert.Multiple(
            () =>
            {
                Assert.That(point.X, Is.EqualTo(10));
                Assert.That(point.Y, Is.EqualTo(20));
                Assert.That(point.Z, Is.EqualTo(-5));
            }
        );
    }

    [Test]
    public void RunningFlagHelpers_ShouldSetDetectAndClearFlag()
    {
        var direction = DirectionType.West;
        var running = Point3D.SetRunning(direction);

        Assert.Multiple(
            () =>
            {
                Assert.That(Point3D.IsRunning(running), Is.True);
                Assert.That(Point3D.GetBaseDirection(running), Is.EqualTo(DirectionType.West));
            }
        );
    }

    [Test]
    public void TryParse_WithInvalidInput_ShouldReturnFalse()
    {
        var parsed = Point3D.TryParse("(10,20)", null, out var point);

        Assert.Multiple(
            () =>
            {
                Assert.That(parsed, Is.False);
                Assert.That(point, Is.EqualTo(default(Point3D)));
            }
        );
    }
}
