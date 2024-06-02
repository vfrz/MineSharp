namespace MineSharp.Content.Items;

public class IronSwordItemInfo : SwordItemInfo
{
    public override ItemId ItemId => ItemId.IronSword;

    public override short DamageOnEntity => 9;

    public override short Durability => 250;

    public override ToolMaterial Material => ToolMaterial.Iron;
}