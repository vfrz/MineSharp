namespace MineSharp.Content.Items;

public class IronAxeItemInfo : AxeItemInfo
{
    public override ItemId ItemId => ItemId.IronAxe;

    public override short DamageOnEntity => 6;

    public override short Durability => 251;

    protected override ToolMaterial Material => ToolMaterial.Iron;
}