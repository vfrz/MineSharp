namespace MineSharp.Content.Items;

public class StonePickaxeItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.StonePickaxe;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}