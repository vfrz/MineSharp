using MineSharp.Nbt.Tags;
using MineSharp.Numerics;

namespace MineSharp.TileEntities;

public class SignTileEntity : TileEntity
{
    public const string Id = "sign";

    public override string EntityId => Id;

    public string? Text1 { get; set; }
    public string? Text2 { get; set; }
    public string? Text3 { get; set; }
    public string? Text4 { get; set; }

    public SignTileEntity()
    {
    }

    public SignTileEntity(Vector3<int> localPosition) : base(localPosition)
    {
    }

    public override INbtTag ToNbt()
    {
        return CreateBaseNbt()
            .AddTag(new StringNbtTag("Text1", Text1))
            .AddTag(new StringNbtTag("Text2", Text2))
            .AddTag(new StringNbtTag("Text3", Text3))
            .AddTag(new StringNbtTag("Text4", Text4));
    }

    public override void LoadFromNbt(CompoundNbtTag nbtTag)
    {
        base.LoadFromNbt(nbtTag);
        Text1 = nbtTag.Get<StringNbtTag>("Text1").Value;
        Text2 = nbtTag.Get<StringNbtTag>("Text2").Value;
        Text3 = nbtTag.Get<StringNbtTag>("Text3").Value;
        Text4 = nbtTag.Get<StringNbtTag>("Text4").Value;
    }
}