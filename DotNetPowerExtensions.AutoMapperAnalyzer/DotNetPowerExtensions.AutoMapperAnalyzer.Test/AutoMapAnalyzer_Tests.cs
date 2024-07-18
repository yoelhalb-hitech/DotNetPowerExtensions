
namespace DotNetPowerExtensions.AutoMapper;

// Unit tests for the AutoMapAnalyzer class
[TestFixture]
public class AutoMapAnalyzerTests : CodeFixVerifier
{
    [Test]
    public async Task NoDiagnosticsReported_ForValidUsage()
    {
        var test = @"
using System;

[AutoMap(typeof(OtherClass))]
public class MyClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class OtherClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}";

        await VerifyCSharpDiagnosticAsync(test);
    }

    [Test]
    public async Task DiagnosticReported_ForMissingAutoMapAttribute()
    {
        var test = @"
public class MyClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}";

        var expectedDiagnostic = new DiagnosticResult
        {
            Id = "YourDiagnosticId1",
            Message = "AutoMap attribute is not used correctly",
            Severity = DiagnosticSeverity.Error,
            Locations = new[] { new DiagnosticResultLocation("Test0.cs", 3, 2) }
        };

        await VerifyCSharpDiagnosticAsync(test, expectedDiagnostic);
    }

    [Test]
    public async Task DiagnosticReported_ForMissingCommonFieldOrProperty()
    {
        var test = @"
using System;

[AutoMap(typeof(OtherClass))]
public class MyClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class OtherClass
{
    public int Id { get; set; }
}";

        var expectedDiagnostic = new DiagnosticResult
        {
            Id = "YourDiagnosticId2",
            Message = "The classes do not have a common field or property",
            Severity = DiagnosticSeverity.Error,
            Locations = new[] { new DiagnosticResultLocation("Test0.cs", 3, 2) }
        };

        await VerifyCSharpDiagnosticAsync(test, expectedDiagnostic);
    }

    [Test]
    public async Task DiagnosticReported_ForMismatchedFieldOrProperty()
    {
        var test = @"
using System;

[AutoMap(typeof(OtherClass))]
public class MyClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class OtherClass
{
    public int Id { get; set; }
    public int Age { get; set; }
}";

        var expectedDiagnostic = new DiagnosticResult
        {
            Id = "YourDiagnosticId3",
            Message = "The classes have a mismatched field or property",
            Severity = DiagnosticSeverity.Error,
            Locations = new[] { new DiagnosticResultLocation("Test0.cs", 3, 2) }
        };

        await VerifyCSharpDiagnosticAsync(test, expectedDiagnostic);
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
        return new AutoMapAnalyzer();
    }

    // Version after internal
    [Test]
    public async Task NoDiagnosticsReported_ForValidUsageWithMatchingFields()
    {
        var test = @"
using System;

[AutoMap(typeof(OtherClass))]
public class MyClass
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age;
}

public class OtherClass
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age;
}";

        await VerifyCSharpDiagnosticAsync(test);
    }

    [Test]
    public async Task NoDiagnosticsReported_ForValidUsageWithMatchingProperties()
    {
        var test = @"
using System;

[AutoMap(typeof(OtherClass))]
public class MyClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class OtherClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}";

        await VerifyCSharpDiagnosticAsync(test);
    }

    [Test]
    public async Task DiagnosticReported_ForMismatchedFieldType()
    {
        var test = @"
using System;

[AutoMap(typeof(OtherClass))]
public class MyClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class OtherClass
{
    public int Id { get; set; }
    public int Name { get; set; }
}";

        var expectedDiagnostic = new DiagnosticResult
        {
            Id = "YourDiagnosticId3",
            Message = "The classes have a mismatched field or property",
            Severity = DiagnosticSeverity.Error,
            Locations = new[] { new DiagnosticResultLocation("Test0.cs", 3, 2) }
        };

        await VerifyCSharpDiagnosticAsync(test, expectedDiagnostic);
    }

    [Test]
    public async Task DiagnosticReported_ForMismatchedPropertyType()
    {
        var test = @"
using System;

[AutoMap(typeof(OtherClass))]
public class MyClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class OtherClass
{
    public int Id { get; set; }
    public int Name { get; set; }
}";

        var expectedDiagnostic = new DiagnosticResult
        {
            Id = "YourDiagnosticId3",
            Message = "The classes have a mismatched field or property",
            Severity = DiagnosticSeverity.Error,
            Locations = new[] { new DiagnosticResultLocation("Test0.cs", 3, 2) }
        };

        await VerifyCSharpDiagnosticAsync(test, expectedDiagnostic);
    }

    [Test]
    public async Task DiagnosticReported_ForMismatchedFieldAndProperty()
    {
        var test = @"
using System;

[AutoMap(typeof(OtherClass))]
public class MyClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class OtherClass
{
    public int Id { get; set; }
    public int Age;
}";

        var expectedDiagnostic = new DiagnosticResult
        {
            Id = "YourDiagnosticId3",
            Message = "The classes have a mismatched field or property",
            Severity = DiagnosticSeverity.Error,
            Locations = new[] { new DiagnosticResultLocation("Test0.cs", 3, 2) }
        };

        await VerifyCSharpDiagnosticAsync(test, expectedDiagnostic);
    }

    //Version after MustInitialize

    [Test]
    public async Task DiagnosticReported_ForMustInitializeAttributeOnNonPublicProperty()
    {
        var test = @"
using System;

[AutoMap(typeof(OtherClass))]
public class MyClass
{
    [MustInitialize]
    private int Id { get; set; }
}

public class OtherClass
{
    public int Id { get; set; }
}";

        var expectedDiagnostic = new DiagnosticResult
        {
            Id = "YourDiagnosticId4",
            Message = "MustInitialize attribute is not used correctly",
            Severity = DiagnosticSeverity.Error,
            Locations = new[] { new DiagnosticResultLocation("Test0.cs", 5, 6) }
        };

        await VerifyCSharpDiagnosticAsync(test, expectedDiagnostic);
    }

    [Test]
    public async Task NoDiagnosticsReported_ForValidUsageWithInternalFields()
    {
        var test = @"
using System;

[AutoMap(typeof(OtherClass), AllowInternal = true)]
public class MyClass
{
    public int Id { get; set; }
    internal string Name { get; set; }
    internal int Age;
}

public class OtherClass
{
    public int Id { get; set; }
    internal string Name { get; set; }
    internal int Age;
}";

        await VerifyCSharpDiagnosticAsync(test);
    }

    // Version after MightRequire

    [Test]
    public async Task DiagnosticReported_ForMismatchedMightRequireAttributes()
    {
        var test = @"
using System;

[MightRequire(1)]
public class MyClass
{
    public int Id { get; set; }
}

[MightRequire(2)]
public class OtherClass
{
    public int Id { get; set; }
}";

        var expectedDiagnostic = new DiagnosticResult
        {
            Id = "YourDiagnosticId5",
            Message = "The classes have a mismatched MightRequire attribute",
            Severity = DiagnosticSeverity.Error,
            Locations = new[] { new DiagnosticResultLocation("Test0.cs", 3, 2) }
        };

        await VerifyCSharpDiagnosticAsync(test, expectedDiagnostic);
    }
}