
namespace DotNetPowerExtensions.AutoMapper;

[TestFixture]
public class AutoMapFunctionAnalyzerTests : CodeFixVerifier
{
    [Test]
    public async Task NoDiagnosticsReported_ForValidUsage()
    {
        var test = @"
using System;

public class SourceClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class TargetClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Mapper
{
    public static TTarget AutoMap<TSource, TTarget>(TSource source)
        where TTarget : new()
    {
        // implementation
    }
}

public class MyClass
{
    public void MyMethod()
    {
        var source = new SourceClass();
        var target = Mapper.AutoMap<SourceClass, TargetClass>(source);
    }
}";

        await VerifyCSharpDiagnosticAsync(test);
    }

    [Test]
    public async Task DiagnosticReported_ForMissingAutoMapAttribute()
    {
        var test = @"
using System;

public class SourceClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class TargetClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Mapper
{
    public static TTarget AutoMap<TSource, TTarget>(TSource source)
        where TTarget : new()
    {
        // implementation
    }
}

public class MyClass
{
    public void MyMethod()
    {
        var source = new SourceClass();
        var target = Mapper.AutoMap<SourceClass, TargetClass>(source);
    }
}";

        var expectedDiagnostic = new DiagnosticResult
        {
            Id = "YourDiagnosticId",
            Message = "AutoMap function is not used correctly",
            Severity = DiagnosticSeverity.Error,
            Locations = new[] { new DiagnosticResultLocation("Test0.cs", 23, 27) }
        };

        await VerifyCSharpDiagnosticAsync(test, expectedDiagnostic);
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new AutoMapFunctionAnalyzer();
    }
}