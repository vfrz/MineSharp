namespace MineSharp.Content.Items;

public class IronPickaxeItemInfo : PickaxeItemInfo
{
    public override ItemId ItemId => ItemId.IronPickaxe;

    public override short DamageOnEntity => 5;

    public override short Durability => 251;

    protected override ToolMaterial Material => ToolMaterial.Iron;
}