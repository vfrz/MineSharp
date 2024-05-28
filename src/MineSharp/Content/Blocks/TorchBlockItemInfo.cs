namespace MineSharp.Content.Blocks;

public class TorchBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.TorchBlock;

    public override bool InstantDig => true;
}