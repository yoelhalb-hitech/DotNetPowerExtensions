using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetPowerExtensions.MustInitialize
{
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field)]
    public class MustInitializeAttribute : Attribute
    {
    }
}
