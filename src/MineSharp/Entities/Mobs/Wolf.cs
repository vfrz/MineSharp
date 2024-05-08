namespace MineSharp.Entities.Mobs;

public class Wolf : MobEntity
{
    public override MobType Type => MobType.Wolf;
    public override short MaxHealth => (short) (Tamed ? 20 : 8);
    
    public bool Tamed { get; }
}