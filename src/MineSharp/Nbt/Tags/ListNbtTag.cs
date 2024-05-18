namespace MineSharp.Nbt.Tags;

public class ListNbtTag(string? name, TagType tagType, List<INbtTag> tags) : INbtTag
{
    public string? Name { get; } = name;
    public TagType TagType { get; } = tagType;
    public List<INbtTag> Tags { get; } = tags;

    public T Get<T>(int index) where T : INbtTag => (T)Tags[index];
}