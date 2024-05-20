namespace MineSharp.Items.Infos.Items;

public class FlintAndSteelItemInfo : ToolItemInfo
{
    public override ItemId Id => ItemId.FlintAndSteel;
    public override short DamageOnEntity { get; } //TODO
    public override short Durability { get; }
}