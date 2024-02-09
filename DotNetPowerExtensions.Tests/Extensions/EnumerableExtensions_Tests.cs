
using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.Tests.Extensions;

internal class EnumerableExtensions_Tests
{
    [Test]
    public void Test_Empty()
    {
        Assert.Throws<ArgumentNullException>(() => (null as int[])!.Empty());
        Array.Empty<int>().Empty().Should().BeTrue();
        new int[] {123}.Empty().Should().BeFalse();
        new int[] { 123, 456 }.Empty().Should().BeFalse();
    }

    [Test]
    public void Test_NullOrEmpty()
    {
        (null as int[]).NullOrEmpty().Should().BeTrue();
        Array.Empty<int>().NullOrEmpty().Should().BeTrue();
        new int[] { 123 }.NullOrEmpty().Should().BeFalse();
        new int[] { 123, 456 }.NullOrEmpty().Should().BeFalse();
    }

    [Test]
    public void Test_HasOnlyOne()
    {
        (null as int[]).HasOnlyOne().Should().BeFalse();
        Array.Empty<int>().HasOnlyOne().Should().BeFalse();
        new int[] { 123 }.HasOnlyOne().Should().BeTrue();
        new int[] { 123, 456 }.HasOnlyOne().Should().BeFalse();
    }
}
