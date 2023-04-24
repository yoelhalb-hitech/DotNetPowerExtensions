using Moq;
using SequelPay.DotNetPowerExtensions;
using System.Reflection;

namespace DotNetPowerExtensions.Tests;

public class LocalFactory_Tests
{
#pragma warning disable CS0649 // Field never assigned
#pragma warning disable CS0169 // Field never used
    sealed class TestClass
    {
        public int PublicProp { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "CS0649:Fields never assigned", Justification = "Used by reflection")]
        public int PublicField;
        internal int InternalProp { get; set; }
        internal int InternalField;
        private int PrivateProp { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1823:Avoid unused private fields", Justification = "Used by reflection")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by reflection")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Used by reflection")]
        private int PrivateField;
    }
#pragma warning restore CS0649 // Field never assigned
#pragma warning disable CS0169 // Field never used

    [Test]
    public void Test_LocalFactory_Create()
    {
        var mock = new Mock<IServiceProvider>();
        mock.Setup(m => m.GetService(It.IsAny<Type>())).Returns<Type>(t => Activator.CreateInstance(t));

        var factory = new LocalFactory<TestClass>(mock.Object);
        var result = factory.Create(new { PublicProp = 5, PublicField = 10, InternalProp = 15, InternalField = 20, PrivateProp = 25, PrivateField = 30 });

        result.Should().NotBeNull();
        result.Should().BeOfType<TestClass>();

        result!.PublicProp.Should().Be(5);
        result.PublicField.Should().Be(10);
        result.InternalProp.Should().Be(15);
        result.InternalField.Should().Be(20);

        result.GetType().GetProperty("PrivateProp", BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(result).Should().Be(25);
        result.GetType().GetField("PrivateField", BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(result).Should().Be(30);
    }
}
