namespace MineSharp.Items.Infos.Items;

public class CookieItemInfo : FoodItemInfo
{
    public override ItemId Id => ItemId.Cookie;
    public override short HealthRestore { get; } //TODO
}