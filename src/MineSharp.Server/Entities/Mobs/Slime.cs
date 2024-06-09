using MineSharp.Core;

namespace MineSharp.Entities.Mobs;

public class Slime(MinecraftServer server) : MobEntity(server)
{
    public override MobType Type => MobType.Slime;

    public override short MaxHealth
    {
        get
        {
            return Size switch
            {
                SizeType.Small => 1,
                SizeType.Medium => 4,
                SizeType.Big => 16,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public SizeType Size { get; init; }

    //TODO Check if values are correct
    public enum SizeType : byte
    {
        Small = 1,
        Medium = 2,
        Big = 3
    }
}