namespace MineSharp.Content.Blocks;

public class RedstoneTorchBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.RedstoneTorchBlock;

    public override bool InstantDig => true;
}