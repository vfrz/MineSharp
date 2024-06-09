using MineSharp.Core;

namespace MineSharp.Entities.Mobs;

public class Cow(MinecraftServer server) : MobEntity(server)
{
    public override MobType Type => MobType.Cow;

    public override short MaxHealth => 10;
}