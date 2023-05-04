
namespace DotNetPowerExtensions.Reflection.Tests;

public class BindingFlagExtension_Tests
{
    public void TestPublicInstanceMethod() { }
    public string? TestPublicInstanceProperty { get; set; }
    public string? TestPublicInstanceField;
    public event EventHandler? TestPublicInstanceEvent;

    public static void TestPublicStaticMethod() { }
    public static string? TestPublicStaticProperty { get; set; }
    public static string? TestPublicStaticField;
    public static event EventHandler? TestPublicStaticEvent;

    internal void TestInternalInstanceMethod() { }
    internal string? TestInternalInstanceProperty { get; set; }
    internal string? TestInternalInstanceField;
    internal event EventHandler? TestInternalInstanceEvent;

    internal static void TestInternalStaticMethod() { }
    internal static string? TestInternalStaticProperty { get; set; }
    internal static string? TestInternalStaticField;
    internal static event EventHandler? TestInternalStaticEvent;

    protected void TestProtectedInstanceMethod() { }
    protected string? TestProtectedInstanceProperty { get; set; }
    protected string? TestProtectedInstanceField;
    protected event EventHandler? TestProtectedInstanceEvent;

    protected static void TestProtectedStaticMethod() { }
    protected static string? TestProtectedStaticProperty { get; set; }
    protected static string? TestProtectedStaticField;
    protected static event EventHandler? TestProtectedStaticEvent;

    internal protected void TestInternalProtectedInstanceMethod() { }
    internal protected string? TestInternalProtectedInstanceProperty { get; set; }
    internal protected string? TestInternalProtectedInstanceField;
    internal protected event EventHandler? TestInternalProtectedInstanceEvent;

    internal protected static void TestInternalProtectedStaticMethod() { }
    internal protected static string? TestInternalProtectedStaticProperty { get; set; }
    internal protected static string? TestInternalProtectedStaticField;
    internal protected static event EventHandler? TestInternalProtectedStaticEvent;

    private void TestPrivateInstanceMethod() { }
    private string? TestPrivateInstanceProperty { get; set; }
    private string? TestPrivateInstanceField;
    private event EventHandler? TestPrivateInstanceEvent;

    private static void TestPrivateStaticMethod() { }
    private static string? TestPrivateStaticProperty { get; set; }
    private static string? TestPrivateStaticField;
    private static event EventHandler? TestPrivateStaticEvent;

    private protected void TestPrivateProtectedInstanceMethod() { }
    private protected string? TestPrivateProtectedInstanceProperty { get; set; }
    private protected string? TestPrivateProtectedInstanceField;
    private protected event EventHandler? TestPrivateProtectedInstanceEvent;

    private protected static void TestPrivateProtectedStaticMethod() { }
    private protected static string? TestPrivateProtectedStaticProperty { get; set; }
    private protected static string? TestPrivateProtectedStaticField;
    private protected static event EventHandler? TestPrivateProtectedStaticEvent;

    [Test]
    public void Test_AllBindings_WorksWithPublicInstance()
    {
        var methods = this.GetType().GetMembers(BindingFlagsExtensions.AllBindings);

        new[]
        {
            nameof(TestPublicInstanceMethod),
            nameof(TestPublicInstanceProperty),
            nameof(TestPublicInstanceField),
            nameof(TestPublicInstanceEvent),
        }.All(n => methods.Any(m => m.Name == n))
        .Should().BeTrue();
    }

    [Test]
    public void Test_AllBindings_WorksWithPublicStatic()
    {
        var methods = this.GetType().GetMembers(BindingFlagsExtensions.AllBindings);

        new[]
        {
            nameof(TestPublicStaticMethod),
            nameof(TestPublicStaticProperty),
            nameof(TestPublicStaticField),
            nameof(TestPublicStaticEvent),
        }.All(n => methods.Any(m => m.Name == n))
        .Should().BeTrue();
    }

    [Test]
    public void Test_AllBindings_WorksWithInternalInstance()
    {
        var methods = this.GetType().GetMembers(BindingFlagsExtensions.AllBindings);

        new[]
        {
            nameof(TestInternalInstanceMethod),
            nameof(TestInternalInstanceProperty),
            nameof(TestInternalInstanceField),
            nameof(TestInternalInstanceEvent),
        }.All(n => methods.Any(m => m.Name == n))
        .Should().BeTrue();
    }

    [Test]
    public void Test_AllBindings_WorksWithInternalStatic()
    {
        var methods = this.GetType().GetMembers(BindingFlagsExtensions.AllBindings);

        new[]
        {
            nameof(TestInternalStaticMethod),
            nameof(TestInternalStaticProperty),
            nameof(TestInternalStaticField),
            nameof(TestInternalStaticEvent),
        }.All(n => methods.Any(m => m.Name == n))
        .Should().BeTrue();
    }

    [Test]
    public void Test_AllBindings_WorksWithInternalProtectedInstance()
    {
        var methods = this.GetType().GetMembers(BindingFlagsExtensions.AllBindings);

        new[]
        {
            nameof(TestInternalProtectedInstanceMethod),
            nameof(TestInternalProtectedInstanceProperty),
            nameof(TestInternalProtectedInstanceField),
            nameof(TestInternalProtectedInstanceEvent),
        }.All(n => methods.Any(m => m.Name == n))
        .Should().BeTrue();
    }

    [Test]
    public void Test_AllBindings_WorksWithInternalProtectedStatic()
    {
        var methods = this.GetType().GetMembers(BindingFlagsExtensions.AllBindings);

        new[]
        {
            nameof(TestInternalProtectedStaticMethod),
            nameof(TestInternalProtectedStaticProperty),
            nameof(TestInternalProtectedStaticField),
            nameof(TestInternalProtectedStaticEvent),
        }.All(n => methods.Any(m => m.Name == n))
        .Should().BeTrue();
    }

    [Test]
    public void Test_AllBindings_WorksWithPrivateInstance()
    {
        var methods = this.GetType().GetMembers(BindingFlagsExtensions.AllBindings);

        new[]
        {
            nameof(TestPrivateInstanceMethod),
            nameof(TestPrivateInstanceProperty),
            nameof(TestPrivateInstanceField),
            nameof(TestPrivateInstanceEvent),
        }.All(n => methods.Any(m => m.Name == n))
        .Should().BeTrue();
    }

    [Test]
    public void Test_AllBindings_WorksWithPrivateStatic()
    {
        var methods = this.GetType().GetMembers(BindingFlagsExtensions.AllBindings);

        new[]
        {
            nameof(TestPrivateStaticMethod),
            nameof(TestPrivateStaticProperty),
            nameof(TestPrivateStaticField),
            nameof(TestPrivateStaticEvent),
        }.All(n => methods.Any(m => m.Name == n))
        .Should().BeTrue();
    }

    [Test]
    public void Test_AllBindings_WorksWithPrivateProtectedInstance()
    {
        var methods = this.GetType().GetMembers(BindingFlagsExtensions.AllBindings);

        new[]
        {
            nameof(TestPrivateProtectedInstanceMethod),
            nameof(TestPrivateProtectedInstanceProperty),
            nameof(TestPrivateProtectedInstanceField),
            nameof(TestPrivateProtectedInstanceEvent),
        }.All(n => methods.Any(m => m.Name == n))
        .Should().BeTrue();
    }

    [Test]
    public void Test_AllBindings_WorksWithProtectedStatic()
    {
        var methods = this.GetType().GetMembers(BindingFlagsExtensions.AllBindings);

        new[]
        {
            nameof(TestPrivateProtectedStaticMethod),
            nameof(TestPrivateProtectedStaticProperty),
            nameof(TestPrivateProtectedStaticField),
            nameof(TestPrivateProtectedStaticEvent),
        }.All(n => methods.Any(m => m.Name == n))
        .Should().BeTrue();
    }
}
