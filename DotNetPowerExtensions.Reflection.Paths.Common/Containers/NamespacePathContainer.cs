
namespace SequelPay.DotNetPowerExtensions.Reflection.Paths; // Needs to be in this namepsace because of the Outer class

internal partial class Outer<TTypeContainerCache>
{
    public class NamespacePathContainer : PathContainer
    {
        public NamespacePathContainer(string name, PathContainer parent) : base(name, parent) { }
        public override string Splitter => ".";
    }
}