namespace MineSharp.Items.Infos.Items;

public class StonePickaxeItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.StonePickaxe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}