
namespace DotNetPowerExtensions.Reflection.Tests;

public class PropertyInfoExtensions_Tests1
{
    class BaseWithPrivate
    {
        public virtual int TestProp { get; private set; }
    }
    class SubNoPrivate : BaseWithPrivate
    {
        public override int TestProp { get => 10; }
    }
    class SubNew : SubNoPrivate
    {
        public new int TestProp { get; }
    }
    class DerivedFromSubNew : SubNew { }
    class SubNew1 : BaseWithPrivate
    {
        public new int TestProp { get; }
    }
    class DerivedFromSubNew1 : SubNew1 { }


    class SetOnlyPublic
    {
        public int TestProp { set => throw new NotImplementedException(); }
    }
    class DerivedFromSetOnlyPublic : SetOnlyPublic { }

    class SubNewWithPrivate : SubNoPrivate
    {
        public new int TestProp { get; private set; }
    }
    class DerivedFromSubNewWithPrivate : SubNewWithPrivate { }

    class HasBothPublic
    {
        public int TestProp { get; set; }
    }
    class DerivedFromHasBothPublic : HasBothPublic { }


    interface IExplicit
    {
        public virtual int TestProp { get => throw new NotImplementedException(); private set => throw new NotImplementedException(); }
    }
    interface IExplicitSub : IExplicit
    {
        int IExplicit.TestProp { get => 10; }
    }
    class HasExplicit : IExplicit
    {
        int IExplicit.TestProp { get => 10; }
    }
    class HasExplicitSub : IExplicitSub
    {
        int IExplicit.TestProp { get => 20; }
    }

    [Test]
    [TestCase(typeof(BaseWithPrivate), ExpectedResult = typeof(BaseWithPrivate))]
    [TestCase(typeof(SubNoPrivate), ExpectedResult = typeof(BaseWithPrivate))]
    [TestCase(typeof(SubNew), ExpectedResult = null)]
    [TestCase(typeof(DerivedFromSubNew), ExpectedResult = null)]
    [TestCase(typeof(SubNew1), ExpectedResult = null)]
    [TestCase(typeof(DerivedFromSubNew1), ExpectedResult = null)]
    [TestCase(typeof(DerivedFromSubNew1), ExpectedResult = null)]
    [TestCase(typeof(SetOnlyPublic), ExpectedResult = typeof(SetOnlyPublic))]
    [TestCase(typeof(DerivedFromSetOnlyPublic), ExpectedResult = typeof(DerivedFromSetOnlyPublic))]
    [TestCase(typeof(SubNewWithPrivate), ExpectedResult = typeof(SubNewWithPrivate))]
    [TestCase(typeof(DerivedFromSubNewWithPrivate), ExpectedResult = typeof(SubNewWithPrivate))]
    [TestCase(typeof(HasBothPublic), ExpectedResult = typeof(HasBothPublic))]
    [TestCase(typeof(DerivedFromHasBothPublic), ExpectedResult = typeof(DerivedFromHasBothPublic))]
    [TestCase(typeof(IExplicit), ExpectedResult = typeof(IExplicit))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Test method")]
    public Type? Test_GetWritablePropertyInfo(Type type)
    {
        var pi = type.GetProperty(nameof(BaseWithPrivate.TestProp))!;

        var result = pi.GetWritablePropertyInfo();

        return result?.ReflectedType;
    }

    [Test]
    [TestCase(typeof(IExplicitSub), ExpectedResult = typeof(IExplicit))]
    [TestCase(typeof(HasExplicit), ExpectedResult = typeof(IExplicit))]
    [TestCase(typeof(HasExplicitSub), ExpectedResult = typeof(IExplicit))]
    public Type? Test_GetWritablePropertyInfo_WithExplicitImplementation(Type type)
    {
        // Explicit implementation is considered private and has the full name
        var pi = type.GetProperties(BindingFlagsExtensions.AllBindings).First(p => p.Name.EndsWith(nameof(IExplicit.TestProp), StringComparison.Ordinal))!;

        var result = pi.GetWritablePropertyInfo();

        return result?.ReflectedType;
    }

    [Test]
    [TestCase(typeof(BaseWithPrivate), ExpectedResult = true)]
    [TestCase(typeof(SubNoPrivate), ExpectedResult = true)]
    [TestCase(typeof(SubNew), ExpectedResult = false)]
    [TestCase(typeof(DerivedFromSubNew), ExpectedResult = false)]
    [TestCase(typeof(SubNew1), ExpectedResult = false)]
    [TestCase(typeof(DerivedFromSubNew1), ExpectedResult = false)]
    [TestCase(typeof(SubNew1), ExpectedResult = false)]
    [TestCase(typeof(DerivedFromSubNew1), ExpectedResult = false)]
    [TestCase(typeof(SetOnlyPublic), ExpectedResult = false)]
    [TestCase(typeof(DerivedFromSetOnlyPublic), ExpectedResult = false)]
    [TestCase(typeof(SubNewWithPrivate), ExpectedResult = true)]
    [TestCase(typeof(DerivedFromSubNewWithPrivate), ExpectedResult = true)]
    [TestCase(typeof(HasBothPublic), ExpectedResult = true)]
    [TestCase(typeof(DerivedFromHasBothPublic), ExpectedResult = true)]
    [TestCase(typeof(IExplicit), ExpectedResult = true)]
    public bool Test_HasGetAndSet_WhenIncludeBasePrivate(Type type)
        => type.GetProperty(nameof(BaseWithPrivate.TestProp), BindingFlagsExtensions.AllBindings)!.HasGetAndSet(true);

    [Test]
    [TestCase(typeof(BaseWithPrivate), ExpectedResult = true)]
    [TestCase(typeof(SubNoPrivate), ExpectedResult = false)]
    [TestCase(typeof(SubNew), ExpectedResult = false)]
    [TestCase(typeof(DerivedFromSubNew), ExpectedResult = false)]
    [TestCase(typeof(SubNew1), ExpectedResult = false)]
    [TestCase(typeof(DerivedFromSubNew1), ExpectedResult = false)]
    [TestCase(typeof(SubNew1), ExpectedResult = false)]
    [TestCase(typeof(DerivedFromSubNew1), ExpectedResult = false)]
    [TestCase(typeof(SetOnlyPublic), ExpectedResult = false)]
    [TestCase(typeof(DerivedFromSetOnlyPublic), ExpectedResult = false)]
    [TestCase(typeof(SubNewWithPrivate), ExpectedResult = true)]
    [TestCase(typeof(DerivedFromSubNewWithPrivate), ExpectedResult = false)]
    [TestCase(typeof(HasBothPublic), ExpectedResult = true)]
    [TestCase(typeof(DerivedFromHasBothPublic), ExpectedResult = true)]
    [TestCase(typeof(IExplicit), ExpectedResult = true)]
    public bool Test_HasGetAndSet_WhenNotIncludeBasePrivate(Type type)
        => type.GetProperty(nameof(BaseWithPrivate.TestProp), BindingFlagsExtensions.AllBindings)!.HasGetAndSet(false);

    [Test]
    [TestCase(typeof(IExplicitSub), true, ExpectedResult = true)]
    [TestCase(typeof(IExplicitSub), false, ExpectedResult = false)]
    [TestCase(typeof(HasExplicit), true, ExpectedResult = true)]
    [TestCase(typeof(HasExplicit), false, ExpectedResult = false)]
    [TestCase(typeof(HasExplicitSub), true, ExpectedResult = true)]
    [TestCase(typeof(HasExplicitSub), false, ExpectedResult = false)]
    public bool Test_HasGetAndSet_WithExplicitImplementation(Type type, bool includeBasePrivate)
          => type.GetProperties(BindingFlagsExtensions.AllBindings)
                .First(p => p.Name.EndsWith(nameof(BaseWithPrivate.TestProp), StringComparison.Ordinal))
                .HasGetAndSet(includeBasePrivate);

    [Test]
    [TestCase(typeof(BaseWithPrivate), ExpectedResult = false)]
    [TestCase(typeof(SubNoPrivate), ExpectedResult = false)]
    [TestCase(typeof(SubNew), ExpectedResult = false)]
    [TestCase(typeof(DerivedFromSubNew), ExpectedResult = false)]
    [TestCase(typeof(SubNew1), ExpectedResult = false)]
    [TestCase(typeof(DerivedFromSubNew1), ExpectedResult = false)]
    [TestCase(typeof(SubNew1), ExpectedResult = false)]
    [TestCase(typeof(DerivedFromSubNew1), ExpectedResult = false)]
    [TestCase(typeof(SetOnlyPublic), ExpectedResult = false)]
    [TestCase(typeof(DerivedFromSetOnlyPublic), ExpectedResult = false)]
    [TestCase(typeof(SubNewWithPrivate), ExpectedResult = false)]
    [TestCase(typeof(DerivedFromSubNewWithPrivate), ExpectedResult = false)]
    [TestCase(typeof(HasBothPublic), ExpectedResult = false)]
    [TestCase(typeof(DerivedFromHasBothPublic), ExpectedResult = false)]
    [TestCase(typeof(IExplicit), ExpectedResult = false)]
    [TestCase(typeof(IExplicitSub), ExpectedResult = true)]
    [TestCase(typeof(HasExplicit), ExpectedResult = true)]
    [TestCase(typeof(HasExplicitSub), ExpectedResult = true)]
    public bool Test_IsExplicitImplementation(Type type)
        => type.GetProperties(BindingFlagsExtensions.AllBindings) // Remember that an explicit implementation has a more complicated name
            .First(p => p.Name.EndsWith(nameof(BaseWithPrivate.TestProp), StringComparison.Ordinal))!
            .IsExplicitImplementation();
}
