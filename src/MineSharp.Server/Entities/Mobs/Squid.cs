using MineSharp.Core;

namespace MineSharp.Entities.Mobs;

public class Squid(MinecraftServer server) : MobEntity(server)
{
    public override MobType Type => MobType.Squid;
    public override short MaxHealth => 10;
}