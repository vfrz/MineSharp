using MineSharp.Core;

namespace MineSharp.Entities.Mobs;

public class Ghast(MinecraftServer server) : MobEntity(server)
{
    public override MobType Type => MobType.Ghast;
    public override short MaxHealth => 10;
}