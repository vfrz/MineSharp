namespace MineSharp.Content.Items;

public class StoneSwordItemInfo : SwordItemInfo
{
    public override ItemId ItemId => ItemId.StoneSword;

    public override short DamageOnEntity => 7;

    public override short Durability => 131;

    protected override ToolMaterial Material => ToolMaterial.Stone;
}