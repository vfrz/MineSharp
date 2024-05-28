namespace MineSharp.Content.Blocks;

public class WheatBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.WheatBlock;

    public override bool InstantDig => true;
}