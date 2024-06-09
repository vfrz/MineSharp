using MineSharp.Core;

namespace MineSharp.Entities.Mobs;

public class Creeper(MinecraftServer server) : MobEntity(server)
{
    public override MobType Type => MobType.Creeper;
    public override short MaxHealth => 20;
}