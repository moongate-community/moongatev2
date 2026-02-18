using System.Reflection;
using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Data.Packets;

namespace Moongate.Tests.Network.Packets;

public class PacketDefinitionTests
{
    [Test]
    public void PacketDefinition_ShouldMatchAllPacketHandlerOpCodes()
    {
        var packetTypes = typeof(PacketDefinition).Assembly
                                                  .GetTypes()
                                                  .Where(static type => type.IsClass && !type.IsAbstract)
                                                  .Select(
                                                      static type =>
                                                          new
                                                          {
                                                              Type = type,
                                                              Attribute = type.GetCustomAttribute<PacketHandlerAttribute>()
                                                          }
                                                  )
                                                  .Where(static x => x.Attribute is not null)
                                                  .ToArray();

        var fields = typeof(PacketDefinition).GetFields(BindingFlags.Public | BindingFlags.Static);
        var constants = fields.ToDictionary(
            static field => field.Name,
            static field => (byte)field.GetRawConstantValue()!
        );

        foreach (var entry in packetTypes)
        {
            var packetName = entry.Type.Name;
            var opCode = entry.Attribute!.OpCode;

            Assert.That(
                constants.TryGetValue(packetName, out var definedOpCode),
                Is.True,
                $"Missing PacketDefinition constant for '{packetName}'."
            );
            Assert.That(
                definedOpCode,
                Is.EqualTo(opCode),
                $"PacketDefinition.{packetName} opcode mismatch."
            );
        }
    }
}
