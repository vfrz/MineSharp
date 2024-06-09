using MineSharp.Core;

namespace MineSharp.Entities.Mobs;

public class Skeleton(MinecraftServer server) : MobEntity(server)
{
    public override MobType Type => MobType.Skeleton;
    public override short MaxHealth => 20;
}