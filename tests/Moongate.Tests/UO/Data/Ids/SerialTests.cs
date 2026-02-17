using Moongate.UO.Data.Ids;

namespace Moongate.Tests.UO.Data.Ids;

public class SerialTests
{
    [Test]
    public void Parse_WhenHexString_ShouldReturnExpectedValue()
    {
        var serial = Serial.Parse("0x40000001");

        Assert.That(serial.Value, Is.EqualTo(0x40000001));
        Assert.That(serial.IsItem, Is.True);
        Assert.That(serial.IsMobile, Is.False);
    }

    [Test]
    public void Parse_WhenDecimalString_ShouldReturnExpectedValue()
    {
        var serial = Serial.Parse("12345");

        Assert.That(serial.Value, Is.EqualTo(12345u));
        Assert.That(serial.IsMobile, Is.True);
    }

    [Test]
    public void Parse_WhenInvalidString_ShouldThrowFormatException()
    {
        Assert.That(() => Serial.Parse("invalid"), Throws.TypeOf<FormatException>());
    }

    [Test]
    public void TryParse_WhenHexString_ShouldSucceed()
    {
        var success = Serial.TryParse("0x0000000A", null, out var serial);

        Assert.That(success, Is.True);
        Assert.That(serial.Value, Is.EqualTo(10u));
    }

    [Test]
    public void TryParse_WhenInvalidString_ShouldReturnFalse()
    {
        var success = Serial.TryParse("0xZZ", null, out var serial);

        Assert.That(success, Is.False);
        Assert.That(serial, Is.EqualTo(default(Serial)));
    }

    [Test]
    public void ToString_ShouldFormatAsEightDigitHex()
    {
        var serial = new Serial(0x2A);

        Assert.That(serial.ToString(), Is.EqualTo("0x0000002A"));
    }

    [Test]
    public void Operators_ShouldSupportArithmeticAndComparison()
    {
        var left = new Serial(10);
        var right = new Serial(3);

        Assert.Multiple(
            () =>
            {
                Assert.That((left + right).Value, Is.EqualTo(13u));
                Assert.That((left - right).Value, Is.EqualTo(7u));
                Assert.That(left > right, Is.True);
                Assert.That(left >= right, Is.True);
                Assert.That(right < left, Is.True);
                Assert.That(right <= left, Is.True);
                Assert.That(left == 10u, Is.True);
                Assert.That(left != 11u, Is.True);
            }
        );
    }

    [Test]
    public void Flags_ShouldExposeValidRangeSemantics()
    {
        Assert.Multiple(
            () =>
            {
                Assert.That(Serial.Zero.IsValid, Is.False);
                Assert.That(Serial.MinusOne.IsValid, Is.True);
                Assert.That(Serial.MinusOne.IsItem, Is.False);
                Assert.That(Serial.ItemOffsetSerial.IsItem, Is.True);
                Assert.That(new Serial(Serial.MobileStart).IsMobile, Is.True);
            }
        );
    }
}
