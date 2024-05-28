namespace MineSharp.Content.Blocks;

public class DeadShrubBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.DeadShrubBlock;

    public override bool InstantDig => true;
}