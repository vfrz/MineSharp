using MineSharp.Sdk.Core;

namespace MineSharp.Content;

public static class ItemIdExtensions
{
    public static bool IsBlock(this ItemId itemId) => itemId is >= ItemId.StoneBlock and <= ItemId.TrapdoorBlock;

    public static bool IsItem(this ItemId itemId) => itemId is >= ItemId.IronShovel and <= ItemId.MusicDiskCat;
}