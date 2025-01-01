extern alias Features;
extern alias Workspaces;

global using System.Reflection;
global using System.Composition;

global using Workspaces::Microsoft.CodeAnalysis;
global using Workspaces::Microsoft.CodeAnalysis.Host;
global using Workspaces::Microsoft.CodeAnalysis.Host.Mef;
global using Workspaces::Microsoft.CodeAnalysis.Shared.Extensions;

global using Microsoft.CodeAnalysis.Text;

global using Features::Microsoft.CodeAnalysis.Completion;

global using DotNetPowerExtensions.MustInitialize.Analyzers;
