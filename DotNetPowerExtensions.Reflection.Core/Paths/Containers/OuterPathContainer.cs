
namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Paths; // Needs to be in this namepsace because of the Outer class

internal partial class Outer<TTypeContainerCache>
{
    public class OuterPathContainer : PathContainer
    {
        public OuterPathContainer(string name, PathContainer parent) : base(name, parent) { }
        public override string Splitter => "+";
    }
}
