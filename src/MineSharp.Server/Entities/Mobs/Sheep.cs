using MineSharp.Core;
using MineSharp.Entities.Metadata;

namespace MineSharp.Entities.Mobs;

public class Sheep : MobEntity
{
    private const byte ColorMetadataIndex = 16;

    public override MobType Type => MobType.Sheep;
    public override short MaxHealth => 8;

    public ColorType Color
    {
        get
        {
            if (MetadataContainer.TryGet<EntityByteMetadata>(ColorMetadataIndex, out var metadata))
                return (ColorType)metadata!.Value;
            return default;
        }
        private set => MetadataContainer.Set(ColorMetadataIndex, new EntityByteMetadata((byte)value));
    }

    public Sheep(MinecraftServer server, ColorType color = ColorType.White) : base(server)
    {
        Color = color;
    }

    public async Task SetColorAsync(ColorType color)
    {
        Color = color;
        await BroadcastMetadataAsync();
    }

    [Flags]
    public enum ColorType : byte
    {
        White = 0,
        Yellow = 1,
        Magenta = 2,
        LightBlue = 3,
        DarkYellow = 4,
        Lime = 5,
        Pink = 6,
        Gray = 7,
        LightGray = 8,
        Cyan = 9,
        Purple = 10,
        Blue = 11,
        Brown = 12,
        Green = 13,
        Red = 14,
        Black = 15,
        Sheared = 16
    }
}