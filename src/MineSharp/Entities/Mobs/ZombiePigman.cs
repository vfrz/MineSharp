using MineSharp.Core;

namespace MineSharp.Entities.Mobs;

public class ZombiePigman(MinecraftServer server) : MobEntity(server)
{
    public override MobType Type => MobType.ZombiePigman;
    public override short MaxHealth => 20;
}