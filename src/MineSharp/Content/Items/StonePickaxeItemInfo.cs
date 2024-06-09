namespace MineSharp.Content.Items;

public class StonePickaxeItemInfo : PickaxeItemInfo
{
    public override ItemId ItemId => ItemId.StonePickaxe;

    public override short DamageOnEntity => 4;

    public override short Durability => 132;

    public override ToolMaterial Material => ToolMaterial.Stone;
}