using MineSharp.Core;

namespace MineSharp.Entities.Mobs;

public class Spider(MinecraftServer server) : MobEntity(server)
{
    public override MobType Type => MobType.Spider;
    public override short MaxHealth => 16;
}