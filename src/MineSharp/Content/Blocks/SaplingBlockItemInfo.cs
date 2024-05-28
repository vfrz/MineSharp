namespace MineSharp.Content.Blocks;

public class SaplingBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.SaplingBlock;

    public override bool InstantDig => true;
}