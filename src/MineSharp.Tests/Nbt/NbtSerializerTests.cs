using FluentAssertions;
using MineSharp.Nbt;
using MineSharp.Nbt.Tags;

namespace MineSharp.Tests.Nbt;

[TestClass]
public class NbtSerializerTests
{
    [TestMethod]
    public void NbtSerializer_SerializeDeserialize()
    {
        var originalNbt = new CompoundNbtTag("Test")
            .AddTag(new ByteArrayNbtTag("Hello", [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]))
            .AddTag(new ByteArrayNbtTag("World", [10, 11, 12, 13, 14, 15, 16, 17, 18, 19]));

        var serialized = NbtSerializer.Serialize(originalNbt);

        var deserialized = (CompoundNbtTag) NbtSerializer.Deserialize(serialized);

        deserialized.Name.Should().Be("Test");
        deserialized.Should().HaveCount(2);
    }
}