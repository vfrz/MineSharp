namespace MineSharp.Content.Items;

public class FlintAndSteelItemInfo : ToolItemInfo
{
    public override ItemId ItemId => ItemId.FlintAndSteel;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}