using MineSharp.Core;

namespace MineSharp.Entities.Mobs;

public class Zombie(MinecraftServer server) : MobEntity(server)
{
    public override MobType Type => MobType.Zombie;
    public override short MaxHealth => 20;
}