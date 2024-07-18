using DotNetPowerExtensions.Analyzers.Throws;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using SequelPay.DotNetPowerExtensions;
using System.Diagnostics;
using System.Reflection;

namespace DataGraphViewer
{
    public partial class Form1 : Form
    {
        private static SemanticModel GetSemanticModel(SyntaxTree tree) => CSharpCompilation.Create("Test",
                    syntaxTrees: new[] { tree },
                    references: new[]
                    {
                                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
                                MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
                                MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location),
                                MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
                                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                                MetadataReference.CreateFromFile(typeof(ThrowsAttribute).Assembly.Location),
                    })
                .GetSemanticModel(tree);
        private static AssignmentDataFlowOperationWalker.DataFlowContext GetContext(string methodBody, string parameters = "")
        {
            var tree = SyntaxFactory.ParseSyntaxTree($$"""
            using System;
            using System.Threading.Tasks;
            using System.Linq;
            using System.Collections.Generic;
            #nullable enable
            class Test
            {
                public Task TestMethod({{parameters}})
                {
                    {{methodBody}}
                }

                public static void Main(){}
            }
        """);

            return GetContextFromSyntaxTree(tree);
        }

        private static AssignmentDataFlowOperationWalker.DataFlowContext GetContextFromSyntaxTree(SyntaxTree tree)
        {
            var semanticModel = GetSemanticModel(tree);

            var method = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();

            return new AssignmentDataFlowOperationWalker.DataFlowContext(semanticModel.Compilation)
            {
                StartOperation = semanticModel.GetOperation(method)!,
            };
        }

        private const string demo = $$"""
                    var c = (((Task?)null)!) ?? (((Task?)null)!);
                    var t = c;
                    var g = (true == true ? (t ?? throw new Exception()) : null);
                    return g;
        """;

        public Form1()
        {
            InitializeComponent();
        }

        private void gViewer1_Load(object sender, EventArgs e)
        {
            bool singleLine = false, useList = true;
            var type = useList ? "List<Action>" : "[]";
            var toType = useList ? "ToList" : "ToArray";
            var inner = singleLine ? $$"""return new {{type}}{a}.Union(new {{type}}{arg2}).Append(arg3).Select(x => x).{{toType}}().First();""" : $$"""
            var a1 = new {{type}}{a};
            var b1 = a1.Union(new {{type}}{arg2}).Append(arg3).Select(x => x).{{toType}}();
            var b = b1.First();
            return b;
        """;
            var context = GetContext($$"""
            return Task.Run(() => {
                {{inner}}
            });
            """, "Action a, Action arg2");

            context.StartingPointPredicate = o => (o is IMethodBodyOperation, ((o as IMethodBodyOperation)?.Syntax as MethodDeclarationSyntax)?.ParameterList.Parameters
                                                    .Select(p => context.Compilation.GetSemanticModel(context.Compilation.SyntaxTrees.First()).GetDeclaredSymbol(p))
                                                    .OfType<ISymbol>()
                                                    .ToArray()
                                                 ?? Array.Empty<ISymbol>());
            context.EndPointPredicate = o => o is IReturnOperation;

            var result = AssignmentDataFlowOperationWalker.Analyze(context);

            var i = 0;
            var startSymbolNodes = result.StartingSymbols.ToDictionary(s => s,
                s => new Node((++i).ToString()) { LabelText = s.Name + Environment.NewLine + " Type: " + s.GetType().Name, },
                SymbolEqualityComparer.Default);
            var symbolNodes = result.SymbolWithOperations.Keys.Except(result.StartingSymbols, SymbolEqualityComparer.Default).ToDictionary(s => s,
                s => new Node((++i).ToString()) { LabelText = s.Name + Environment.NewLine + " Type: " + s.GetType().Name, },
                SymbolEqualityComparer.Default);
            var startOperationsNodes = result.StartingOperations.ToDictionary(o => o, o => new Node((++i).ToString())
            { LabelText = o.GetType().Name.Replace("Operation", "") + Environment.NewLine + o.Syntax.ToFullString(), });
            var operationsNodes = result.FlowOperations.Keys.Union(result.OperationsWithSymbols.Keys).Union(result.EndingOperations).Except(result.StartingOperations)
                    .ToDictionary(o => o, o => new Node((++i).ToString()) { LabelText = o.GetType().Name.Replace("Operation", "") + Environment.NewLine + o.Syntax.ToFullString(), });

            //v
            var graph = new Graph();
            startSymbolNodes.Values.ToList().ForEach(n =>
            {
                n.Label.FontSize = 10;
                n.Attr.FillColor = Microsoft.Msagl.Drawing.Color.White;
                n.Attr.Shape = Microsoft.Msagl.Drawing.Shape.Triangle;
                graph.AddNode(n);
            });

            symbolNodes.Values.ToList().ForEach(n =>
            {
                n.Label.FontSize = 10;
                n.Attr.FillColor = Microsoft.Msagl.Drawing.Color.White;
                n.Attr.Shape = Microsoft.Msagl.Drawing.Shape.Trapezium;
                graph.AddNode(n);
            });


            //var subgraph = new Subgraph("start operations");
            //subgraph.Label.Text = "start operations";
            //// startOperationsNodes.ToList().ForEach(n => subgraph.AddNode(n));
            //graph.RootSubgraph.AddSubgraph(subgraph);

            startOperationsNodes.Values.ToList().ForEach(n =>
            {
                n.Label.FontSize = 10;
                n.Attr.FillColor = Microsoft.Msagl.Drawing.Color.White;
                n.Attr.Shape = Microsoft.Msagl.Drawing.Shape.Diamond;
                graph.AddNode(n);
            });

            operationsNodes.ToList().ForEach(n =>
            {
                n.Value.Label.FontSize = 10;
                n.Value.Attr.FillColor = Microsoft.Msagl.Drawing.Color.White;
                n.Value.Attr.Shape = result.EndingOperations.Contains(n.Key) ? Microsoft.Msagl.Drawing.Shape.House : Microsoft.Msagl.Drawing.Shape.Circle;
                graph.AddNode(n.Value);
            });

            foreach (var o in operationsNodes)
            {
                if(result.FlowOperations.ContainsKey(o.Key))
                    foreach (var f in result.FlowOperations[o.Key])
                    {
                        if(startOperationsNodes.ContainsKey(f)) graph.AddEdge(startOperationsNodes[f].Id, o.Value.Id);
                        else graph.AddEdge(operationsNodes[f].Id, o.Value.Id);
                    }

                if (result.OperationsWithSymbols.ContainsKey(o.Key))
                    foreach (var f in result.OperationsWithSymbols[o.Key])
                    {
                        if (startSymbolNodes.ContainsKey(f)) graph.AddEdge(startSymbolNodes[f].Id, o.Value.Id);
                        else graph.AddEdge(symbolNodes[f].Id, o.Value.Id);
                    }
            }


            foreach (var s in symbolNodes)
            {
                if (result.SymbolWithOperations.ContainsKey(s.Key))
                    foreach (var f in result.SymbolWithOperations[s.Key])
                    {
                        if (startOperationsNodes.ContainsKey(f)) graph.AddEdge(startOperationsNodes[f].Id, s.Value.Id);
                        else graph.AddEdge(operationsNodes[f].Id, s.Value.Id);
                    }
               }

            //gViewer1.AddEdge()
            gViewer1.Graph = graph;
            gViewer1.ObjectUnderMouseCursorChanged +=
                (sender, args) =>
                {
                    if (args.NewObject is not DNode node) return;
                    gViewer1.Graph.Nodes.ToList().ForEach(n => n.Label.FontColor = Microsoft.Msagl.Drawing.Color.Black);
                    gViewer1.Graph.Edges.ToList().ForEach(e => e.Attr.LineWidth = 5);
                    gViewer1.Graph.Edges.ToList().ForEach(e => e.Attr.Color = Microsoft.Msagl.Drawing.Color.Black);
                    //Debugger.Break();

                    UpdateIn(node.Node);

                };
           // graph.GeometryGraph.BoundingBox = new Microsoft.Msagl.Core.Geometry.Rectangle { Height = graph.GeometryGraph.BoundingBox.Height, Left = gViewer1.Left -10, Right = gViewer1.Right - 10 };

        }

        private void UpdateIn(Node node)
        {
            node.InEdges.ToList().ForEach(edge =>
            {
                edge.Attr.Color = Microsoft.Msagl.Drawing.Color.Red;
                edge.Attr.LineWidth = 20;
                var source = gViewer1.Graph.FindNode(edge.Source);
                if (source.Label.FontColor == Microsoft.Msagl.Drawing.Color.Red) return;

                source.Label.FontColor = Microsoft.Msagl.Drawing.Color.Red;
                UpdateIn(source);
            });
        }
    }
}