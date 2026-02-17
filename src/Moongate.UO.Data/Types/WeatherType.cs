namespace Moongate.UO.Data.Types;

public enum WeatherType : byte
{
    /*
     * Types: 0x00 - "It starts to rain",
     * 0x01 - "A fierce storm approaches.",
     * 0x02 - "It begins to snow",
     * 0x03 - "A storm is brewing.",
     * 0xFF - None (turns off sound effects),
     * 0xFE (no effect?? Set temperature?)
     */

    None = 0xFF,
    Rain = 0x00,
    Storm = 0x01,
    Snow = 0x02,
    NoEffect = 0xFE
}
