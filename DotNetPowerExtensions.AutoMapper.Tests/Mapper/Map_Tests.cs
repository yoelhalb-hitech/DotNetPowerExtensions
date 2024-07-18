using Map = SequelPay.DotNetPowerExtensions.AutoMapper.Mapper;

namespace DotNetPowerExtensions.AutoMapper.Tests.Mapper;

internal class Map_Tests
{
    [Test]
    public void AutoMap_CopiesPropertiesFromSourceToTarget()
    {
        var source = new SourceClass
        {
            Id = 1,
            Name = "John Doe"
        };

        var target = Map.AutoMap<SourceClass, TargetClass>(source);

        Assert.AreEqual(source.Id, target.Id);
        Assert.AreEqual(source.Name, target.Name);
    }

    [Test]
    public void AutoMap_CopiesFieldsFromSourceToTarget()
    {
        var source = new SourceClass
        {
            Id = 1,
            Name = "John Doe"
        };

        var target = Map.AutoMap<SourceClass, TargetClass>(source);

        Assert.AreEqual(source.Age, target.Age);
    }


[Test]
public void AutoMap_ValidUsage()
{
    var source = new SourceClass();
    var target = Map.AutoMap<SourceClass, TargetClass>(source);

    Assert.IsNotNull(target);
    Assert.AreEqual(source.Id, target.Id);
    Assert.AreEqual(source.Name, target.Name);
}

[Test]
public void AutoMap_ValidUsageWithInternalFields()
{
    var source = new SourceClass();
    var target = Map.AutoMap<SourceClass, TargetClassWithInternalFields>(source);

    Assert.IsNotNull(target);
    Assert.AreEqual(source.Id, target.Id);
    Assert.AreEqual(source.Name, target.Name);
    Assert.AreEqual(source.Age, target.Age);
}

[Test]
public void AutoMap_ValidUsageWithInternalProperties()
{
    var source = new SourceClass();
    var target = Map.AutoMap<SourceClass, TargetClassWithInternalProperties>(source);

    Assert.IsNotNull(target);
    Assert.AreEqual(source.Id, target.Id);
    Assert.AreEqual(source.Name, target.Name);
    Assert.AreEqual(source.Age, target.Age);
}

[Test]
public void AutoMap_MissingAutoMapAttribute()
{
    var source = new SourceClass();
    var target = Map.AutoMap<SourceClass, TargetClassWithoutAutoMapAttribute>(source);

    Assert.IsNull(target);
}

[Test]
public void AutoMap_MismatchedFieldType()
{
    var source = new SourceClass();
    var target = Mapper.AutoMap<SourceClass, TargetClassWithMismatchedFieldType>(source);

    Assert.IsNull(target);
}

[Test]
public void AutoMap_MismatchedPropertyType()
{
    var source = new SourceClass();
    var target = Map.AutoMap<SourceClass, TargetClassWithMismatchedPropertyType>(source);

    Assert.IsNull(target);
}

[Test]
public void AutoMap_ValidUsage()
{
    var source = new SourceClass();
    var target = Map.AutoMap<SourceClass, TargetClass>(source);

    Assert.IsNotNull(target);
    Assert.AreEqual(source.Id, target.Id);
    Assert.AreEqual(source.Name, target.Name);
}

[Test]
public void AutoMap_ValidUsageWithInternalFields()
{
    var source = new SourceClass();
    var target = Map.AutoMap<SourceClass, TargetClassWithInternalFields>(source);

    Assert.IsNotNull(target);
    Assert.AreEqual(source.Id, target.Id);
    Assert.AreEqual(source.Name, target.Name);
    Assert.AreEqual(source.Age, target.Age);
}

[Test]
public void AutoMap_ValidUsageWithInternalProperties()
{
    var source = new SourceClass();
    var target = Map.AutoMap<SourceClass, TargetClassWithInternalProperties>(source);

    Assert.IsNotNull(target);
    Assert.AreEqual(source.Id, target.Id);
    Assert.AreEqual(source.Name, target.Name);
    Assert.AreEqual(source.Age, target.Age);
}

[Test]
public void AutoMap_MissingAutoMapAttribute()
{
    var source = new SourceClass();
    var target = Mapper.AutoMap<SourceClass, TargetClassWithoutAutoMapAttribute>(source);

    Assert.IsNull(target);
}

[Test]
public void AutoMap_MismatchedFieldType()
{
    var source = new SourceClass();
    var target = Mapper.AutoMap<SourceClass, TargetClassWithMismatchedFieldType>(source);

    Assert.IsNull(target);
}

[Test]
public void AutoMap_MismatchedPropertyType()
{
    var source = new SourceClass();
    var target = Mapper.AutoMap<SourceClass, TargetClassWithMismatchedPropertyType>(source);

    Assert.IsNull(target);
}

[Test]
public void AutoMap_ValidUsageWithGeneratedClass()
{
    var source = new SourceClass();
    var target = Mapper.AutoMap<SourceClass, GeneratedObject>(source, "GeneratedObject");

    Assert.IsNotNull(target);
    Assert.AreEqual(source.Id, target.Id);
    Assert.AreEqual(source.Name, target.Name);
    Assert.AreEqual(source.Age, target.Age);
}

}
