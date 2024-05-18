using MineSharp.Core;

namespace MineSharp.Entities.Mobs;

public class Pig(MinecraftServer server) : MobEntity(server)
{
    public override MobType Type => MobType.Pig;
    public override short MaxHealth => 10;
}