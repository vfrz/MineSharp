using FluentAssertions;
using MineSharp.World.Generation;

namespace MineSharp.Tests.World.Generation;

[TestClass]
public class PoissonDiscSamplingTests
{
    [TestMethod]
    public void PoissonDiscSampling_BasicTest()
    {
        var sampler = new UniformPoissonDiskSampler(42);

        var points = sampler.SampleRectangle(0, 0, 64, 64, 8);

        points.Should().HaveCountGreaterThan(0);

        points.Should().AllSatisfy(point =>
        {
            point.X.Should().BeInRange(0, 64);
            point.Z.Should().BeInRange(0, 64);
        });
    }
}