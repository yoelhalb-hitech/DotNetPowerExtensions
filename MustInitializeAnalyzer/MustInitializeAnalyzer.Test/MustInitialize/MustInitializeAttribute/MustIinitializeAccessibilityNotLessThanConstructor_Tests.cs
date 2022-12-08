using Microsoft;
using DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.Test.MustInitialize.MustInitializeAttribute;

internal class MustIinitializeAccessibilityNotLessThanConstructor_Tests : AnalyzerVerifierBase<MustIinitializeAccessibilityNotLessThanConstructor>
{
    public static string[] Suffixes = { "", "Attribute", "()", "Attribute()" };
    public static string[] Prefixes = {"", "DotNetPowerExtensions.MustInitialize.",
                                                                    "global::DotNetPowerExtensions.MustInitialize." };
    public static List<string> AccessTypes = new List<string> { "private", "private protected", "protected", "internal", "internal protected", "public" };
    public static List<string> AccessTypesWithEmpty => AccessTypes.Union(new[] { "" }).ToList();

    public static List<string> ClassAccessTypes = new List<string> { "internal", "public" };

    [Test]
    public async Task Test_DoesNotWarn_WhenNoMustInitialize()
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
        public class DeclareType
        {
            public DeclareType(){}
            private string TestProp { get; set; }
            private string TestField;
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class MustInitializeAttribute : System.Attribute {}
        public class DeclareType
        {
            public DeclareType(){}
            [MustInitialize{{suffix}}] private string TestProp { get; set; }
            [MustInitialize{{suffix}}] private string TestField;
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    #region Non Diagnostics

    [Test]
    public async Task Test_MustInitialize_NoDiagnostic_OnEqualToType_InnerClassNoCtor(
                            [ValueSource(nameof(ClassAccessTypes))] string outerAccess,
                            [ValueSource(nameof(AccessTypesWithEmpty))] string access,
                            [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
        {{outerAccess}} class Outer
        {
            {{access}} class TypeName
            {
                [{{prefix}}MustInitialize{{suffix}}] {{access}} string TestProp { get; set; }
                [{{prefix}}MustInitialize{{suffix}}] {{access}} string TestField;
            }
        }
        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_NoDiagnostic_OnEqualToType_OuterClassNoCtor([ValueSource(nameof(ClassAccessTypes))] string access,
                        [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;     
        
        {{access}} class TypeName
        {
            [{{prefix}}MustInitialize{{suffix}}] {{access}} string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] {{access}} string TestField;
        }    
        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_NoDiagnostic_OnEqualToCtor_InnerClassOneCtor(
                        [ValueSource(nameof(ClassAccessTypes))] string outerAccess,
                        [ValueSource(nameof(AccessTypesWithEmpty))] string innerAccess,
                        [ValueSource(nameof(AccessTypesWithEmpty))] string access,
                        [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
        {{outerAccess}} class Outer
        {
            {{innerAccess}} class TypeName
            {
                {{access}} TypeName(){}
                [{{prefix}}MustInitialize{{suffix}}] {{access}} string TestProp { get; set; }
                [{{prefix}}MustInitialize{{suffix}}] {{access}} string TestField;
            }
        }
        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_NoDiagnostic_OnEqualToCtor_OuterClassOneCtor([ValueSource(nameof(ClassAccessTypes))] string classAccess,
                        [ValueSource(nameof(AccessTypesWithEmpty))] string access,
                        [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;     
        
        {{classAccess}} class TypeName
        {
            {{access}} TypeName(){}
            [{{prefix}}MustInitialize{{suffix}}] {{access}} string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] {{access}} string TestField;
        }    
        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_NoDiagnostic_OnEqualToOneCtor_InnerClassMultipleCtors(
                    [ValueSource(nameof(ClassAccessTypes))] string outerAccess,
                    [ValueSource(nameof(AccessTypesWithEmpty))] string innerAccess,
                    [ValueSource(nameof(AccessTypesWithEmpty))] string access,
                    [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
        {{outerAccess}} class Outer
        {
            {{innerAccess}} class TypeName
            {
                {{access}} TypeName(){}
                [{{prefix}}MustInitialize{{suffix}}] {{access}} string TestProp { get; set; }
                [{{prefix}}MustInitialize{{suffix}}] {{access}} string TestField;
            }
        }
        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_NoDiagnostic_OnEqualAllCtors_OuterClassMultipleCtors([ValueSource(nameof(ClassAccessTypes))] string classAccess,
                        [ValueSource(nameof(AccessTypesWithEmpty))] string access,
                        [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;     
        
        {{classAccess}} class TypeName
        {
            {{access}} TypeName(string t){}
            {{access}} TypeName(){}
            [{{prefix}}MustInitialize{{suffix}}] {{access}} string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] {{access}} string TestField;
        }    
        """;


        await VerifyAnalyzerAsync(test);
    }

    #endregion


    #region Diagnostics    

    [Test]
    public async Task Test_MustInitialize_Diagnostic_OnLessThenType_InnerClassNoCtor(
                            [ValueSource(nameof(ClassAccessTypes))] string outerAccess,
                            [ValueSource(nameof(AccessTypes))] string innerAccess,
                            [ValueSource(nameof(AccessTypes))] string propAccess,
                            [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(AccessTypes.IndexOf(propAccess) < AccessTypes.IndexOf(innerAccess) || propAccess == "internal" && innerAccess == "protected");

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
        {{outerAccess}} class Outer
        {
            {{innerAccess}} class TypeName
            {
                [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestProp { get; set; }
                [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestField;
            }
        }
        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_Diagnostic_OnLessThenType_OuterClassNoCtor([ValueSource(nameof(ClassAccessTypes))] string classAccess,
                        [ValueSource(nameof(AccessTypes))] string propAccess,
                        [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(AccessTypes.IndexOf(propAccess) < AccessTypes.IndexOf(classAccess) || propAccess == "internal" && classAccess == "protected");

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;     

        {{classAccess}} class TypeName
        {
            [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestProp { get; set; }
            [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestField;
        }    
        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_Diagnostic_OnLessThenCtor_InnerClassOneCtor(
                        [ValueSource(nameof(ClassAccessTypes))] string outerAccess,
                        [ValueSource(nameof(AccessTypes))] string innerAccess,
                        [ValueSource(nameof(AccessTypes))] string propAccess,
                        [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(AccessTypes.IndexOf(propAccess) < AccessTypes.IndexOf(innerAccess) || propAccess == "internal" && innerAccess == "protected");

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        {{outerAccess}} class Outer
        {
            {{innerAccess}} class TypeName
            {
                {{innerAccess}} TypeName(){}
                [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestProp { get; set; }
                [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestField;
            }
        }
        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_Diagnostic_OnLessThenCtor_OuterClassOneCtor([ValueSource(nameof(ClassAccessTypes))] string classAccess,
                        [ValueSource(nameof(AccessTypes))] string propAccess,
                        [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(AccessTypes.IndexOf(propAccess) < AccessTypes.IndexOf(classAccess) || propAccess == "internal" && classAccess == "protected");

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;     

        {{classAccess}} class TypeName
        {
            {{classAccess}} TypeName(){}
            [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestProp { get; set; }
            [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestField;
        }    
        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_Diagnostic_OnLessThenAllCtors_InnerClassMultipleCtors(
                    [ValueSource(nameof(ClassAccessTypes))] string outerAccess,
                    [ValueSource(nameof(AccessTypes))] string innerAccess,
                    [ValueSource(nameof(AccessTypes))] string propAccess,
                    [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(AccessTypes.IndexOf(propAccess) < AccessTypes.IndexOf(innerAccess) || propAccess == "internal" && innerAccess == "protected");

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        {{outerAccess}} class Outer
        {
            {{innerAccess}} class TypeName
            {
                public TypeName(string t){}
                {{innerAccess}} TypeName(){}
                [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestProp { get; set; }
                [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestField;
            }
        }
        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_Diagnostic_OnLessThenCtors_OuterClassMultipleCtors([ValueSource(nameof(ClassAccessTypes))] string classAccess,
                        [ValueSource(nameof(AccessTypes))] string propAccess,
                        [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(AccessTypes.IndexOf(propAccess) < AccessTypes.IndexOf(classAccess) || propAccess == "internal" && classAccess == "protected");

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        {{classAccess}} class TypeName
        {
            public TypeName(string t){}
            {{classAccess}} TypeName(){}
            [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestProp { get; set; }
            [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestField;
        }    
        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_Diagnostic_OnSetLessThenType_InnerClassNoCtor(
                        [ValueSource(nameof(ClassAccessTypes))] string outerAccess,
                        [ValueSource(nameof(AccessTypes))] string innerAccess,
                        [ValueSource(nameof(AccessTypes))] string propAccess,
                        [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(AccessTypes.IndexOf(propAccess), Is.LessThan(AccessTypes.IndexOf(innerAccess)));
        Assume.That(propAccess != "protected" || innerAccess != "internal"); //Otherwise we can have a compile error

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
        {{outerAccess}} class Outer
        {
            {{innerAccess}} class TypeName
            {
                [[|{{prefix}}MustInitialize{{suffix}}|]] {{innerAccess}} string TestProp { get; {{propAccess}} set; }
                [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestField;
            }
        }
        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_Diagnostic_OnSetLessThenType_OuterClassNoCtor([ValueSource(nameof(ClassAccessTypes))] string classAccess,
                        [ValueSource(nameof(AccessTypes))] string propAccess,
                        [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(AccessTypes.IndexOf(propAccess), Is.LessThan(AccessTypes.IndexOf(classAccess)));
        Assume.That(propAccess != "protected" || classAccess != "internal"); //Otherwise we can have a compile error

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;     

        {{classAccess}} class TypeName
        {
            [[|{{prefix}}MustInitialize{{suffix}}|]] {{classAccess}} string TestProp { get; {{propAccess}} set; }
            [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestField;
        }    
        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_Diagnostic_OnSetLessThenCtor_InnerClassOneCtor(
                        [ValueSource(nameof(ClassAccessTypes))] string outerAccess,
                        [ValueSource(nameof(AccessTypes))] string innerAccess,
                        [ValueSource(nameof(AccessTypes))] string propAccess,
                        [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(AccessTypes.IndexOf(propAccess), Is.LessThan(AccessTypes.IndexOf(innerAccess)));
        Assume.That(propAccess != "protected" || innerAccess != "internal"); //Otherwise we can have a compile error

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        {{outerAccess}} class Outer
        {
            {{innerAccess}} class TypeName
            {
                {{innerAccess}} TypeName(){}
                [[|{{prefix}}MustInitialize{{suffix}}|]] {{innerAccess}} string TestProp { get; {{propAccess}} set; }
                [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestField;
            }
        }
        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_Diagnostic_OnSetLessThenCtor_OuterClassOneCtor([ValueSource(nameof(ClassAccessTypes))] string classAccess,
                        [ValueSource(nameof(AccessTypes))] string propAccess,
                        [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(AccessTypes.IndexOf(propAccess), Is.LessThan(AccessTypes.IndexOf(classAccess)));
        Assume.That(propAccess != "protected" || classAccess != "internal"); //Otherwise we can have a compile error

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;     

        {{classAccess}} class TypeName
        {
            {{classAccess}} TypeName(){}
            [[|{{prefix}}MustInitialize{{suffix}}|]] {{classAccess}} string TestProp { get; {{propAccess}} set; }
            [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestField;
        }    
        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_Diagnostic_OnSetLessThenAllCtors_InnerClassMultipleCtors(
                    [ValueSource(nameof(ClassAccessTypes))] string outerAccess,
                    [ValueSource(nameof(AccessTypes))] string innerAccess,
                    [ValueSource(nameof(AccessTypes))] string propAccess,
                    [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(AccessTypes.IndexOf(propAccess), Is.LessThan(AccessTypes.IndexOf(innerAccess)));
        Assume.That(propAccess != "protected" || innerAccess != "internal"); //Otherwise we can have a compile error

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        {{outerAccess}} class Outer
        {
            {{innerAccess}} class TypeName
            {
                public TypeName(string t){}
                {{innerAccess}} TypeName(){}
                [[|{{prefix}}MustInitialize{{suffix}}|]] {{innerAccess}} string TestProp { get; {{propAccess}} set; }
                [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestField;
            }
        }
        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_Diagnostic_OnSetLessThenCtors_OuterClassMultipleCtors([ValueSource(nameof(ClassAccessTypes))] string classAccess,
                        [ValueSource(nameof(AccessTypes))] string propAccess,
                        [ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(AccessTypes.IndexOf(propAccess), Is.LessThan(AccessTypes.IndexOf(classAccess)));
        Assume.That(propAccess != "protected" || classAccess != "internal"); //Otherwise we can have a compile error

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;     

        {{classAccess}} class TypeName
        {
            public TypeName(string t){}
            {{classAccess}} TypeName(){}
            [[|{{prefix}}MustInitialize{{suffix}}|]] {{classAccess}} string TestProp { get; {{propAccess}} set; }
            [[|{{prefix}}MustInitialize{{suffix}}|]] {{propAccess}} string TestField;
        }    
        """;


        await VerifyAnalyzerAsync(test);
    }

    #endregion

    [Test]
    public async Task Test_NoDiagnostic_WithInterface([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        // Because it depends on the ctor we don't care on interfaces
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] internal string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_Works_WhenNoAccessSpecified([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        // Remember that class default is internal while member default is private
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        class DeclareType
        {            
            [[|{{prefix}}MustInitialize{{suffix}}|]] string TestProp { get; set; }
            [[|{{prefix}}MustInitialize{{suffix}}|]] string TestField;  
        }        
        """;

        await VerifyAnalyzerAsync(test);
    }
}
