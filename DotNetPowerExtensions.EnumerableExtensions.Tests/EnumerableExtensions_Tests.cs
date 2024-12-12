using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.Tests.Extensions;

internal class EnumerableExtensions_Tests
{
    internal static readonly int[] ArrayWithOne = [123];
    internal static readonly int[] ArrayWithTwo = [123, 456];

    [Test]
    public void Test_Empty()
    {
        Assert.Throws<ArgumentNullException>(() => (null as int[])!.Empty());
        Array.Empty<int>().Empty().Should().BeTrue();
        ArrayWithOne.Empty().Should().BeFalse();
        ArrayWithTwo.Empty().Should().BeFalse();
    }

    [Test]
    public void Test_NullOrEmpty()
    {
        (null as int[]).NullOrEmpty().Should().BeTrue();
        Array.Empty<int>().NullOrEmpty().Should().BeTrue();
        ArrayWithOne.NullOrEmpty().Should().BeFalse();
        ArrayWithTwo.NullOrEmpty().Should().BeFalse();
    }

    [Test]
    public void Test_HasOnlyOne()
    {
        (null as int[]).HasOnlyOne().Should().BeFalse();
        Array.Empty<int>().HasOnlyOne().Should().BeFalse();
        ArrayWithOne.HasOnlyOne().Should().BeTrue();
        ArrayWithTwo.HasOnlyOne().Should().BeFalse();
    }
}
