using Map = SequelPay.DotNetPowerExtensions.AutoMapper.Mapper;

namespace DotNetPowerExtensions.AutoMapper.Tests.Mapper;

internal class Map_ValidationTests
{
    [Test]
    public void AutoMap_ThrowsException_WhenAutoMapAttributeIsMissing()
    {
        var source = new SourceClass
        {
            Id = 1,
            Name = "John Doe"
        };

        Assert.Throws<InvalidOperationException>(() => Map.AutoMap<SourceClass, InvalidTargetClass>(source));
    }
}
