namespace MineSharp.Content.Items;

public class CookieItemInfo : FoodItemInfo
{
    public override ItemId ItemId => ItemId.Cookie;
    public override short HealthRestore { get; } //TODO
}