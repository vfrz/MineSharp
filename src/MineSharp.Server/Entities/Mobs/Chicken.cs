using MineSharp.Core;

namespace MineSharp.Entities.Mobs;

public class Chicken(MinecraftServer server) : MobEntity(server)
{
    public override MobType Type => MobType.Chicken;
    public override short MaxHealth => 4;
}