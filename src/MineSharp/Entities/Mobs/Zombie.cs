namespace MineSharp.Entities.Mobs;

public class Zombie : MobEntity
{
    public override MobType Type => MobType.Zombie;
    public override short MaxHealth => 20;
}