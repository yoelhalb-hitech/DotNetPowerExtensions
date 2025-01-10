using SequelPay.DotNetPowerExtensions.Collections;

namespace DotNetPowerExtensions.EnumerableExtensions.Tests.Collections;

internal class FollowThroughEnumerable_Tests
{
    public class Base { }
    public class Derived : Base { }
    public class Derived2 : Derived { }
    public class Derived3 : Derived2 { }

    [Test]
    public void Test_First()
    {
        var followThrough = new FollowThroughEnumerable<Type>(typeof(Derived3), t => t.BaseType);
        followThrough.First().Should().Be(typeof(Derived3));
    }

    [Test]
    public void Test_Last()
    {
        var followThrough = new FollowThroughEnumerable<Type>(typeof(Derived3), t => t.BaseType);
        followThrough.Last().Should().Be(typeof(object));
    }

    [Test]
    public void Test_WithStopFunc()
    {
        var followThrough = new FollowThroughEnumerable<Type>(typeof(Derived3), t => t.BaseType, t => t.BaseType == typeof(object));
        followThrough.Last().Should().Be(typeof(Base));
    }

    [Test]
    public void Test_WithoutStopFunc()
    {
        var followThrough = new FollowThroughEnumerable<Type>(typeof(Derived3), t => t.BaseType);
        followThrough.Last().Should().Be(typeof(object));
    }
}
