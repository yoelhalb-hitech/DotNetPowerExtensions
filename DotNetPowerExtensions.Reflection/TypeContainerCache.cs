using SequelPay.DotNetPowerExtensions.Reflection.Core.Models;
using SequelPay.DotNetPowerExtensions.Reflection.Models;
using SequelPay.DotNetPowerExtensions.Reflection.Core.Paths;
using System.Text.RegularExpressions;

namespace SequelPay.DotNetPowerExtensions.Reflection;

internal class TypeContainerCache : Outer<TypeContainerCache>.TypeContainerCache
{
    private Dictionary<string, ITypeDetailInfo> specialTypes = new Dictionary<string, ITypeDetailInfo>
    {
        ["string"] = typeof(string).GetTypeDetailInfo(),
        ["object"] = typeof(object).GetTypeDetailInfo(),
        ["int"] = typeof(int).GetTypeDetailInfo(),
        ["short"] = typeof(short).GetTypeDetailInfo(),
        ["long"] = typeof(long).GetTypeDetailInfo(),
        ["char"] = typeof(char).GetTypeDetailInfo(),
        ["byte"] = typeof(byte).GetTypeDetailInfo(),
        ["sbyte"] = typeof(sbyte).GetTypeDetailInfo(),
        ["ushort"] = typeof(ushort).GetTypeDetailInfo(),
        ["uint"] = typeof(uint).GetTypeDetailInfo(),
        ["nint"] = typeof(nint).GetTypeDetailInfo(),
        ["nuint"] = typeof(nuint).GetTypeDetailInfo(),
        ["ulong"] = typeof(ulong).GetTypeDetailInfo(),
        ["float"] = typeof(float).GetTypeDetailInfo(),
        ["double"] = typeof(double).GetTypeDetailInfo(),
        ["decimal"] = typeof(decimal).GetTypeDetailInfo(),
        ["bool"] = typeof(bool).GetTypeDetailInfo(),
    };
    protected override Dictionary<string, ITypeDetailInfo> SpecialTypes => specialTypes;

    private static Regex fileClassRegex = new Regex(@"(^|\.|\+)<([^>]+)>[^_]+__([^+]+)");
    internal static string FixForFileClass(string path) => fileClassRegex.Replace(path, "$1<$2>+$3");

    internal static List<Assembly> processedAssemblies = new();
    
    internal override void Ensure()
    {
        var notProcessed = AppDomain.CurrentDomain.GetAssemblies().Except(processedAssemblies);

        foreach (var assembly in notProcessed)
        {
            assembly.GetTypes().ToList().ForEach(t =>
            {
                // TODO... if there is a name collision in 2 assmeblies?
                try 
                {
                    var container = InsertPathContainer(GetFullName(t));
                    pathTypeDict[container.FullPath] = t;
                } 
                catch { }
            });
        }

        processedAssemblies.AddRange(notProcessed);
    }
    private static string GetFullName(Type type)
        => FixForFileClass(type.FullName ?? throw new TypeLoadException("Not a valid type"));

    internal override string GetFullName(ITypeDetailInfo type)
        => GetFullName((type as TypeDetailInfo)?.Type ?? throw new TypeLoadException("Not a valid type"));

    internal override ITypeDetailInfo GetTypeDetailInfo(string fullPath) => pathTypeDict[fullPath].GetTypeDetailInfo();

    private static Dictionary<string, Type> pathTypeDict = new Dictionary<string, Type>();
}
