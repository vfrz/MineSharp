using MineSharp.Core;

namespace MineSharp.Entities.Mobs;

public class GiantZombie(MinecraftServer server) : MobEntity(server)
{
    public override MobType Type => MobType.GiantZombie;
    public override short MaxHealth => 100;
}