using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TestHelper;

namespace MustInitializeAnalyzer.Test
{
    [TestClass]
    public class MustInitializeAnalyzer_Tests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MustInitializeAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new MustInitializeCodeFixProvider();
        }

        public class CodeGen
        {
            public string ClassModifier { get; set; }
            public string AttrName { get; set; }
            public string PropModifier { get; set; }
            public string PropBody { get; set; }
            public string ExtraProp { get; set; }
            public string InitCode { get; set; }
            public string ExtraCode { get; set; }
            public string ExtraUsing { get; set; }
        }

        string GetCodeString(CodeGen codeGen) => $@"
    using System;   
    using MustInitializeAnalyzer;
    {codeGen.ExtraUsing}

    namespace ConsoleApplication1
    {{
        {codeGen.ClassModifier} class TypeName
        {{
            [{codeGen.AttrName}] {codeGen.PropModifier} string Test {codeGen.PropBody}
            {codeGen.ExtraProp}
            public static void Testing() => new TypeName{{{codeGen.InitCode}}};

            

            {codeGen.ExtraCode}
        }}
    }}";

        const string ReadWriteAutoProperty = "{ get; set; }";
        const string ReadWriteField = ";";

        const string OtherMustInitializer = "[AttributeUsage(AttributeTargets.All)]public class MustInitializeAttribute : Attribute {}";

        [TestMethod]
        [DataRow("MustInitialize", ReadWriteAutoProperty)]
        [DataRow("MustInitializeAttribute", ReadWriteAutoProperty)]
        [DataRow("MustInitializeAnalyzer.MustInitializeAttribute", ReadWriteAutoProperty)]
        [DataRow("mu.MustInitializeAttribute", ReadWriteAutoProperty, "using mu = MustInitializeAnalyzer;")]
        [DataRow("MustInitialize", ReadWriteField)]
        [DataRow("MustInitializeAttribute", ReadWriteField)]
        [DataRow("MustInitializeAnalyzer.MustInitializeAttribute", ReadWriteField)]
        [DataRow("mu.MustInitializeAttribute", ReadWriteField, "using mu = MustInitializeAnalyzer;")]
        public void Test_MustInitialize_AddsDiagnostic(string attrName, string propDefinition, string extraUsing = null)
        {
            var test = GetCodeString(new CodeGen 
            {
                AttrName = attrName,
                PropModifier = "public",
                PropBody = propDefinition,
                ExtraUsing = extraUsing,
            });
            var expected = new DiagnosticResult
            {
                Id = "MustInitialize",
                Message = "Property 'Test' must be initialized",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 45)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var testCodeFix = GetCodeString(new CodeGen {
                AttrName = attrName, PropModifier = "public", PropBody = propDefinition, InitCode = " Test = default " });
            VerifyCSharpFix(test, testCodeFix);
        }

        [TestMethod]
        [DataRow("MustInitialize", ReadWriteAutoProperty)]
        [DataRow("MustInitializeAttribute", ReadWriteAutoProperty)]
        [DataRow("MustInitialize", ReadWriteField)]
        [DataRow("MustInitializeAttribute", ReadWriteField)]        
        public void Test_MustInitialize_Other_DoesNotAddDiagnostic(string attrName, string propDefinition)
        {
            var test = GetCodeString(new CodeGen { 
                AttrName = attrName, PropModifier = "public", PropBody = propDefinition, ExtraCode = OtherMustInitializer 
            });

            VerifyCSharpDiagnostic(test); // Shoult not have
        }        
    }
}
