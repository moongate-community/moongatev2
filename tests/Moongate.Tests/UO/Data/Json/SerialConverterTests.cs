using System.Text;
using System.Text.Json;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Json.Converters;

namespace Moongate.Tests.UO.Data.Json;

public class SerialConverterTests
{
    [Test]
    public void Deserialize_NumberToken_WithHighUInt32Value_ShouldSucceed()
    {
        var payload = Encoding.UTF8.GetBytes("4294967295");
        var reader = new Utf8JsonReader(payload);
        reader.Read();
        var converter = new SerialConverter();

        var serial = converter.Read(ref reader, typeof(Serial), new JsonSerializerOptions());

        Assert.That(serial.Value, Is.EqualTo(uint.MaxValue));
    }
}
