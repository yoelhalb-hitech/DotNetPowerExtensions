
namespace SequelPay.DotNetPowerExtensions.Reflection.Paths; // Needs to be in this namepsace because of the Outer class

internal partial class Outer<TTypeContainerCache>
{
    public class GenericStubPathContainer : PathContainer
    {
        public GenericStubPathContainer(string name, ITypeDetailInfo type) : base(name, null)
        {
            Type = type;
        }
        public override string Splitter => "";
        public override ITypeDetailInfo? Type { get; }
    }
}
