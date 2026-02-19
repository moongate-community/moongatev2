namespace Moongate.Server.Data.Config;

public class MoongateGameConfig
{
    public string ShardName { get; set; } = "Moongate Shard";

    public int TimerTickMilliseconds { get; set; } = 250;

    public int TimerWheelSize { get; set; } = 512;
}
