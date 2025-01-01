
namespace SequelPay.DotNetPowerExtensions.Reflection.Paths; // Needs to be in this namepsace because of the Outer class

internal partial class Outer<TTypeContainerCache>
{
    public class TypePathContainer : PathContainer
    {
        public TypePathContainer(string name) : base(name, null) { }
        public override string Splitter => "";
    }
}
