﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;

public abstract class ByAttributeAnalyzerBase : MustInitializeAnalyzerBase
{
    protected abstract Type AttributeType { get; }
    protected virtual string DescriptiveName => AttributeType.Name.Replace(typeof(Attribute).Name, "");

    protected virtual INamedTypeSymbol[] GetAttributeSymbol(INamedTypeSymbol[] typeSymbols)
        => typeSymbols.Where(s => s.Name == AttributeType.Name).ToArray();
}
