using System;
using System.Collections.Generic;
using System.Text;

namespace MustInitializeAnalyzer
{
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field)]
    public class MustInitializeAttribute : Attribute
    {
    }
}
