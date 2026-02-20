using Moongate.Network.Packets.Incoming.System;
using UOMap = Moongate.UO.Data.Maps.Map;

namespace Moongate.Network.Packets.Outgoing.World;

/// <summary>
/// Creates commonly used General Information packets.
/// </summary>
public static class GeneralInformationFactory
{
    public static GeneralInformationPacket CreateSetCursorHueSetMap(byte mapId)
        => GeneralInformationPacket.CreateSetCursorHueSetMap(mapId);

    public static GeneralInformationPacket CreateSetCursorHueSetMap(UOMap? map)
        => GeneralInformationPacket.CreateSetCursorHueSetMap(map);
}
