using MineSharp.Sdk.Core;

namespace MineSharp.Content.Items;

public class SignItemInfo : ItemInfo
{
    public override ItemId ItemId => ItemId.Sign;

    public override byte StackMax => 1;
}