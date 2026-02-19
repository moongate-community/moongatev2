namespace Moongate.Network.Packets.Generators.Data.Internal;

internal sealed class PacketModel
{
    public PacketModel(string typeName, string packetName, byte opCode, bool isFixed, int length, string? description)
    {
        TypeName = typeName;
        PacketName = packetName;
        OpCode = opCode;
        IsFixed = isFixed;
        Length = length;
        Description = description;
    }

    public string TypeName { get; }
    public string PacketName { get; }
    public byte OpCode { get; }
    public bool IsFixed { get; }
    public int Length { get; }
    public string? Description { get; }
}
