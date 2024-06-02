namespace MineSharp.Content.Items;

public class GoldenSwordItemInfo : SwordItemInfo
{
    public override ItemId ItemId => ItemId.GoldenSword;

    public override short DamageOnEntity => 5;

    public override short Durability => 32;

    public override ToolMaterial Material => ToolMaterial.Gold;
}