
namespace DotNetPowerExtensions.Reflection.Paths.Tests;

file class TestFile { }

internal class PathMatcher_Tests
{
    class PathMatcherInnerType { public class Inner { } }
    class Inner { }

    class InnerGeneric<T>
    {
        public class NonGenericInner { }
        public class GenericInner<T1> { }
    }
    class InnerGeneric<T, T1> { }

    const string minimalNS = $"{nameof(Tests)}";
    const string withMinimalNS = $"{minimalNS}.{nameof(PathMatcher_Tests)}";
    const string withHalfNS = $"{nameof(Reflection)}.{withMinimalNS}";
    const string withFullNS = $"{nameof(DotNetPowerExtensions)}.{withHalfNS}";
    const string fullNS = $"{nameof(DotNetPowerExtensions)}.{nameof(Reflection)}.{minimalNS}";

    [Test]
    [TestCase($"{withFullNS}+{nameof(PathMatcherInnerType)}", ExpectedResult = typeof(PathMatcherInnerType))]
    [TestCase($"{withFullNS}+{nameof(PathMatcherInnerType)}+{nameof(Inner)}", ExpectedResult = typeof(PathMatcherInnerType.Inner))]
    [TestCase($"{withFullNS}+{nameof(Inner)}", ExpectedResult = typeof(Inner))]
    [TestCase($"{withHalfNS}+{nameof(PathMatcherInnerType)}", ExpectedResult = typeof(PathMatcherInnerType))]
    [TestCase($"{withHalfNS}+{nameof(PathMatcherInnerType)}+{nameof(Inner)}", ExpectedResult = typeof(PathMatcherInnerType.Inner))]
    [TestCase($"{withHalfNS}+{nameof(Inner)}", ExpectedResult = typeof(Inner))]
    [TestCase($"{withMinimalNS}+{nameof(PathMatcherInnerType)}", ExpectedResult = typeof(PathMatcherInnerType))]
    [TestCase($"{withMinimalNS}+{nameof(PathMatcherInnerType)}+{nameof(Inner)}", ExpectedResult = typeof(PathMatcherInnerType.Inner))]
    [TestCase($"{withMinimalNS}+{nameof(Inner)}", ExpectedResult = typeof(Inner))]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(PathMatcherInnerType)}", ExpectedResult = typeof(PathMatcherInnerType))]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(PathMatcherInnerType)}+{nameof(Inner)}",
            ExpectedResult = typeof(PathMatcherInnerType.Inner))]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(Inner)}", ExpectedResult = typeof(Inner))]
    [TestCase($"{nameof(PathMatcherInnerType)}", ExpectedResult = typeof(PathMatcherInnerType))]
    [TestCase($"{nameof(TestFile)}", ExpectedResult = typeof(TestFile))]
    [TestCase($"<{nameof(PathMatcher_Tests)}_cs>+{nameof(TestFile)}", ExpectedResult = typeof(TestFile))]
    [TestCase($"{minimalNS}.<{nameof(PathMatcher_Tests)}_cs>+{nameof(TestFile)}", ExpectedResult = typeof(TestFile))]
    public Type Test_InnerType(string path)
        => (new Outer<TypeContainerCache>.PathMatcher().Match(path) as TypeDetailInfo)!.Type;


    [TestCase($"{withFullNS}+{nameof(InnerGeneric<object>)}`1", ExpectedResult = typeof(InnerGeneric<>))]
    [TestCase($"{withHalfNS}+{nameof(InnerGeneric<object>)}`1", ExpectedResult = typeof(InnerGeneric<>))]
    [TestCase($"{withMinimalNS}+{nameof(InnerGeneric<object>)}`1", ExpectedResult = typeof(InnerGeneric<>))]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}`1", ExpectedResult = typeof(InnerGeneric<>))]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}`1", ExpectedResult = typeof(InnerGeneric<>))]
    [TestCase($"{withFullNS}+{nameof(InnerGeneric<object>)}`2", ExpectedResult = typeof(InnerGeneric<,>))]
    [TestCase($"{withHalfNS}+{nameof(InnerGeneric<object>)}`2", ExpectedResult = typeof(InnerGeneric<,>))]
    [TestCase($"{withMinimalNS}+{nameof(InnerGeneric<object>)}`2", ExpectedResult = typeof(InnerGeneric<,>))]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}`2", ExpectedResult = typeof(InnerGeneric<,>))]
    [TestCase($"{nameof(InnerGeneric<object>)}`2", ExpectedResult = typeof(InnerGeneric<,>))]
    public Type Test_GenericType(string path)
        => (new Outer<TypeContainerCache>.PathMatcher().Match(path) as TypeDetailInfo)!.Type;

    [TestCase($"{withFullNS}+{nameof(InnerGeneric<object>)}<string>", ExpectedResult = typeof(InnerGeneric<string>))]
    [TestCase($"{withHalfNS}+{nameof(InnerGeneric<object>)}<int>", ExpectedResult = typeof(InnerGeneric<int>))]
    [TestCase($"{withMinimalNS}+{nameof(InnerGeneric<object>)}<{withMinimalNS}+{nameof(InnerGeneric<object>)}<{nameof(PathMatcherInnerType)}>>", ExpectedResult = typeof(InnerGeneric<InnerGeneric<PathMatcherInnerType>>))]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<{nameof(PathMatcher_Tests)}+{nameof(Inner)}>", ExpectedResult = typeof(InnerGeneric<Inner>))]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<System.String>", ExpectedResult = typeof(InnerGeneric<string>))]
    [TestCase($"{withFullNS}+{nameof(InnerGeneric<object>)}<string, Int32>", ExpectedResult = typeof(InnerGeneric<string, int>))]
    [TestCase($"{withHalfNS}+{nameof(InnerGeneric<object>)}<System.Object, String>", ExpectedResult = typeof(InnerGeneric<object, string>))]
    public Type Test_GenericConstructedType(string path)
        => (new Outer<TypeContainerCache>.PathMatcher().Match(path) as TypeDetailInfo)!.Type;

    [Test]
    [TestCase($"{withFullNS}", ExpectedResult = typeof(PathMatcher_Tests))]
    [TestCase($"{withHalfNS}", ExpectedResult = typeof(PathMatcher_Tests))]
    [TestCase($"{withMinimalNS}", ExpectedResult = typeof(PathMatcher_Tests))]
    [TestCase($"{nameof(PathMatcher_Tests)}", ExpectedResult = typeof(PathMatcher_Tests))]
    [TestCase($"{nameof(PathMatcher_Tests)}[]", ExpectedResult = typeof(PathMatcher_Tests[]))]
    [TestCase($"int", ExpectedResult = typeof(int))]
    [TestCase($"object", ExpectedResult = typeof(object))]
    [TestCase($"string", ExpectedResult = typeof(string))]
    [TestCase($"bool", ExpectedResult = typeof(bool))]
    [TestCase($"decimal", ExpectedResult = typeof(decimal))]
    [TestCase($"decimal[]", ExpectedResult = typeof(decimal[]))]
    [TestCase($"decimal[][]", ExpectedResult = typeof(decimal[][]))]
    public Type Test_OuterType(string path)
        => (new Outer<TypeContainerCache>.PathMatcher().Match(path) as TypeDetailInfo)!.Type;


    [Test]
    public void Test_AmbiguousThrows()
    {
        var ex = Assert.Throws<AmbiguousMatchException>(() => new Outer<TypeContainerCache>.PathMatcher().Match(nameof(Inner)));
        ex.Message.Should().Be($"`{nameof(Inner)}` is ambiguous between `{withFullNS}+{nameof(Inner)}` and `{withFullNS}+{nameof(PathMatcherInnerType)}+{nameof(Inner)}`");
    }

    [Test]
    [TestCase("Not Found")]
    [TestCase("NotFound")]
    public void Test_NotFoundThrows(string path)
    {
        var ex = Assert.Throws<KeyNotFoundException>(() => new Outer<TypeContainerCache>.PathMatcher().Match(path));
        ex.Message.Should().Be($"A type with name `{path}` was not found, please verify that the name is correct and that the type has been loaded");
    }

    [Test]
    [TestCase(typeof(Inner), ExpectedResult = $"{nameof(PathMatcher_Tests)}+{nameof(Inner)}")]
    [TestCase(typeof(PathMatcherInnerType.Inner), ExpectedResult = $"{nameof(PathMatcherInnerType)}+{nameof(Inner)}")]
    [TestCase(typeof(PathMatcherInnerType), ExpectedResult = $"{nameof(PathMatcherInnerType)}")]
    [TestCase(typeof(PathMatcher_Tests), ExpectedResult = $"{nameof(PathMatcher_Tests)}")]
    [TestCase(typeof(TestFile), ExpectedResult = $"{nameof(TestFile)}")]
    [TestCase(typeof(int), ExpectedResult = $"int")]
    [TestCase(typeof(string), ExpectedResult = $"string")]
    [TestCase(typeof(object), ExpectedResult = $"object")]
    [TestCase(typeof(decimal), ExpectedResult = $"decimal")]
    [TestCase(typeof(bool), ExpectedResult = $"bool")]
    [TestCase(typeof(bool[]), ExpectedResult = $"bool[]")]
    [TestCase(typeof(bool[][]), ExpectedResult = $"bool[][]")]
    [TestCase(typeof(PathMatcher_Tests[]), ExpectedResult = $"PathMatcher_Tests[]")]
    [TestCase(typeof(PathMatcher_Tests[][][]), ExpectedResult = $"PathMatcher_Tests[][][]")]
    [TestCase(typeof(List<>), ExpectedResult = $"List`1")]
    [TestCase(typeof(List<int>), ExpectedResult = $"List<int>")]
    [TestCase(typeof(List<Dictionary<string, PathMatcherInnerType>>), ExpectedResult = $"List<Dictionary<string,{nameof(PathMatcherInnerType)}>>")]
    [TestCase(typeof(Dictionary<,>), ExpectedResult = $"Dictionary`2")]
    [TestCase(typeof(Dictionary<string, Inner>), ExpectedResult = $"Dictionary<string,{nameof(PathMatcher_Tests)}+{nameof(Inner)}>")]
    [TestCase(typeof(InnerGeneric<>), ExpectedResult = $"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}`1")]
    [TestCase(typeof(InnerGeneric<string>), ExpectedResult = $"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<string>")]
    [TestCase(typeof(InnerGeneric<InnerGeneric<string>>.NonGenericInner), ExpectedResult = $"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<string>>+{nameof(InnerGeneric<object>.NonGenericInner)}")]
    [TestCase(typeof(InnerGeneric<>.GenericInner<>), ExpectedResult = $"{nameof(InnerGeneric<object>.GenericInner<object>)}`1")]
    [TestCase(typeof(InnerGeneric<int>.GenericInner<string>), ExpectedResult = $"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<int>+{nameof(InnerGeneric<object>.GenericInner<object>)}<string>")]
    [TestCase(typeof(InnerGeneric<InnerGeneric<string>>.GenericInner<string>), ExpectedResult = $"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<string>>+{nameof(InnerGeneric<object>.GenericInner<object>)}<string>")]
    [TestCase(typeof(InnerGeneric<int>.GenericInner<InnerGeneric<string>>), ExpectedResult = $"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<int>+{nameof(InnerGeneric<object>.GenericInner<object>)}<{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<string>>")]
    [TestCase(typeof(InnerGeneric<,>), ExpectedResult = $"{nameof(InnerGeneric<object>)}`2")]
    [TestCase(typeof(InnerGeneric<int, string>), ExpectedResult = $"{nameof(InnerGeneric<object>)}<int,string>")]

    public string Test_GetMinimalPath_Type(Type type)
        => new Outer<TypeContainerCache>.PathMatcher().GetMinimalPath(type.GetTypeDetailInfo());

    [Test]
    [TestCase(typeof(Inner), ExpectedResult = $"{withFullNS}+{nameof(Inner)}")]
    [TestCase(typeof(PathMatcherInnerType.Inner), ExpectedResult = $"{withFullNS}+{nameof(PathMatcherInnerType)}+{nameof(Inner)}")]
    [TestCase(typeof(PathMatcherInnerType), ExpectedResult = $"{withFullNS}+{nameof(PathMatcherInnerType)}")]
    [TestCase(typeof(PathMatcher_Tests), ExpectedResult = $"{withFullNS}")]
    [TestCase(typeof(TestFile), ExpectedResult = $"{fullNS}.<{nameof(PathMatcher_Tests)}_cs>+{nameof(TestFile)}")]
    [TestCase(typeof(int), ExpectedResult = $"System.Int32")]
    [TestCase(typeof(string), ExpectedResult = $"System.String")]
    [TestCase(typeof(object), ExpectedResult = $"System.Object")]
    [TestCase(typeof(decimal), ExpectedResult = $"System.Decimal")]
    [TestCase(typeof(bool), ExpectedResult = $"System.Boolean")]
    [TestCase(typeof(bool[]), ExpectedResult = $"System.Boolean[]")]
    [TestCase(typeof(bool[][]), ExpectedResult = $"System.Boolean[][]")]
    [TestCase(typeof(PathMatcher_Tests[]), ExpectedResult = $"{withFullNS}[]")]
    [TestCase(typeof(PathMatcher_Tests[][][]), ExpectedResult = $"{withFullNS}[][][]")]
    [TestCase(typeof(List<>), ExpectedResult = $"System.Collections.Generic.List`1")]
    [TestCase(typeof(List<int>), ExpectedResult = $"System.Collections.Generic.List<System.Int32>")]
    [TestCase(typeof(List<Dictionary<string, PathMatcherInnerType>>), ExpectedResult = $"System.Collections.Generic.List<System.Collections.Generic.Dictionary<System.String,{withFullNS}+{nameof(PathMatcherInnerType)}>>")]
    [TestCase(typeof(Dictionary<,>), ExpectedResult = $"System.Collections.Generic.Dictionary`2")]
    [TestCase(typeof(Dictionary<string, Inner>), ExpectedResult = $"System.Collections.Generic.Dictionary<System.String,{withFullNS}+{nameof(Inner)}>")]
    [TestCase(typeof(InnerGeneric<>), ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}`1")]
    [TestCase(typeof(InnerGeneric<string>), ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}<System.String>")]
    [TestCase(typeof(InnerGeneric<InnerGeneric<string>>.NonGenericInner), ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}<{withFullNS}+{nameof(InnerGeneric<object>)}<System.String>>+{nameof(InnerGeneric<object>.NonGenericInner)}")]
    [TestCase(typeof(InnerGeneric<>.GenericInner<>), ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}`1+{nameof(InnerGeneric<object>.GenericInner<object>)}`1")]
    [TestCase(typeof(InnerGeneric<int>.GenericInner<string>), ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}<System.Int32>+{nameof(InnerGeneric<object>.GenericInner<object>)}<System.String>")]
    [TestCase(typeof(InnerGeneric<InnerGeneric<string>>.GenericInner<string>), ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}<{withFullNS}+{nameof(InnerGeneric<object>)}<System.String>>+{nameof(InnerGeneric<object>.GenericInner<object>)}<System.String>")]
    [TestCase(typeof(InnerGeneric<int>.GenericInner<InnerGeneric<string>>), ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}<System.Int32>+{nameof(InnerGeneric<object>.GenericInner<object>)}<{withFullNS}+{nameof(InnerGeneric<object>)}<System.String>>")]
    [TestCase(typeof(InnerGeneric<,>), ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}`2")]
    [TestCase(typeof(InnerGeneric<int, string>), ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}<System.Int32,System.String>")]
    public string Test_GetFullPath_Type(Type type)
        => new Outer<TypeContainerCache>.PathMatcher().GetFullPath(type.GetTypeDetailInfo());

    [Test]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(Inner)}", ExpectedResult = $"{withFullNS}+{nameof(Inner)}")]
    [TestCase($"{nameof(PathMatcherInnerType)}+{nameof(Inner)}", ExpectedResult = $"{withFullNS}+{nameof(PathMatcherInnerType)}+{nameof(Inner)}")]
    [TestCase($"{nameof(PathMatcherInnerType)}", ExpectedResult = $"{withFullNS}+{nameof(PathMatcherInnerType)}")]
    [TestCase($"{nameof(PathMatcher_Tests)}", ExpectedResult = $"{withFullNS}")]
    [TestCase($"{nameof(TestFile)}", ExpectedResult = $"{fullNS}.<{nameof(PathMatcher_Tests)}_cs>+{nameof(TestFile)}")]
    [TestCase("int", ExpectedResult = $"System.Int32")]
    [TestCase("string", ExpectedResult = $"System.String")]
    [TestCase("object", ExpectedResult = $"System.Object")]
    [TestCase("decimal", ExpectedResult = $"System.Decimal")]
    [TestCase("bool", ExpectedResult = $"System.Boolean")]
    [TestCase("bool[]", ExpectedResult = $"System.Boolean[]")]
    [TestCase("bool[][]", ExpectedResult = $"System.Boolean[][]")]
    [TestCase($"{nameof(PathMatcher_Tests)}[]", ExpectedResult = $"{withFullNS}[]")]
    [TestCase($"{nameof(PathMatcher_Tests)}[][]", ExpectedResult = $"{withFullNS}[][]")]
    [TestCase("List`1", ExpectedResult = $"System.Collections.Generic.List`1")]
    [TestCase("List<int>", ExpectedResult = $"System.Collections.Generic.List<System.Int32>")]
    [TestCase("List<Dictionary<string, PathMatcherInnerType>>", ExpectedResult = $"System.Collections.Generic.List<System.Collections.Generic.Dictionary<System.String,{withFullNS}+{nameof(PathMatcherInnerType)}>>")]
    [TestCase("Dictionary`2", ExpectedResult = $"System.Collections.Generic.Dictionary`2")]
    [TestCase($"Dictionary<string,{nameof(PathMatcher_Tests)}+{nameof(Inner)}>", ExpectedResult = $"System.Collections.Generic.Dictionary<System.String,{withFullNS}+{nameof(Inner)}>")]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}`1", ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}`1")]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<string>", ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}<System.String>")]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<System.String>>+{nameof(InnerGeneric<object>.NonGenericInner)}", ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}<{withFullNS}+{nameof(InnerGeneric<object>)}<System.String>>+{nameof(InnerGeneric<object>.NonGenericInner)}")]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}`1+{nameof(InnerGeneric<object>.GenericInner<object>)}`1", ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}`1+{nameof(InnerGeneric<object>.GenericInner<object>)}`1")]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<int>+{nameof(InnerGeneric<object>.GenericInner<object>)}<System.String>", ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}<System.Int32>+{nameof(InnerGeneric<object>.GenericInner<object>)}<System.String>")]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<System.String>>+{nameof(InnerGeneric<object>.GenericInner<object>)}<System.String>", ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}<{withFullNS}+{nameof(InnerGeneric<object>)}<System.String>>+{nameof(InnerGeneric<object>.GenericInner<object>)}<System.String>")]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<int>+{nameof(InnerGeneric<object>.GenericInner<object>)}<{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<System.String>>", ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}<System.Int32>+{nameof(InnerGeneric<object>.GenericInner<object>)}<{withFullNS}+{nameof(InnerGeneric<object>)}<System.String>>")]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}`2", ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}`2")]
    [TestCase($"{nameof(PathMatcher_Tests)}+{nameof(InnerGeneric<object>)}<int,string>", ExpectedResult = $"{withFullNS}+{nameof(InnerGeneric<object>)}<System.Int32,System.String>")]

    public string Test_GetFullPath_String(string minimalPath)
        => new Outer<TypeContainerCache>.PathMatcher().GetFullPath(minimalPath);

    [Test]
    public void Test_EmtpyComma()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new Outer<TypeContainerCache>.PathMatcher().Match("Dictionary<,>"));
        ex.Message.Should().Be("Empty path, possibly you have an extra comma in a generic type (Parameter 'path')");
    }

    [Test]
    public void Test_InvalidNamespace()
    {
        var ex = Assert.Throws<FormatException>(() => new Outer<TypeContainerCache>.PathMatcher().Match($"{nameof(PathMatcher_Tests)}.{nameof(Inner)}"));
        ex.Message.Should().Be($"{nameof(PathMatcher_Tests)} is not a namespace of {nameof(Inner)}, if it is an outer type then use `+` as the separator");
    }

    [Test]
    public void Test_InvalidOuter()
    {
        var ex = Assert.Throws<FormatException>(() => new Outer<TypeContainerCache>.PathMatcher().Match($"{withMinimalNS.Replace(".", "+")}"));
        ex.Message.Should().Be($"{nameof(Tests)} is not an outer type of {nameof(PathMatcher_Tests)}, if it is a namespace then use `.` as the separator");
    }

    [Test]
    public void Test_ThrowsForGenericStub()
    {
        var ex = Assert.Throws<KeyNotFoundException>(() => new Outer<TypeContainerCache>.PathMatcher().GetFullPath($"List<T>"));
        ex.Message.Should().Be($"A type with name `T` was not found, please verify that the name is correct and that the type has been loaded");
    }

    [Test]
    public void Test_DoesNotThrow_WhenProvidedGenericStub()
    {
        Assert.DoesNotThrow(() => new Outer<TypeContainerCache>
            .PathMatcher(typeof(List<>).GetGenericArguments().Select(a => a.GetTypeDetailInfo()).ToArray())
            .GetFullPath($"List<T>"));
    }
}
