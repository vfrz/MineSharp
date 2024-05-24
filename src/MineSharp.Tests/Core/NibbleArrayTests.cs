using FluentAssertions;
using MineSharp.Core;

namespace MineSharp.Tests.Core;

[TestClass]
public class NibbleArrayTests
{
    [TestMethod]
    public void NibbleArray_SetAndGet()
    {
        // Arrange
        var innerArray = new byte[1];
        var array = new NibbleArray(innerArray, 0)
        {
            [0] = 6,
            [1] = 9
        };

        // Act
        var firstValue = array[0];
        var secondValue = array[1];

        // Assert
        firstValue.Should().Be(6);
        secondValue.Should().Be(9);
    }
}