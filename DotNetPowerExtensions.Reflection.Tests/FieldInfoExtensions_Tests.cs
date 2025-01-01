
namespace DotNetPowerExtensions.Reflection.Tests;

public class FieldInfoExtensions_Tests
{
    [Test]
    [TestCase(FieldAttributes.Assembly, ExpectedResult = true)]
    [TestCase(FieldAttributes.FamORAssem, ExpectedResult = true)]
    [TestCase(FieldAttributes.FamANDAssem, ExpectedResult = false)]
    [TestCase(FieldAttributes.Family, ExpectedResult = false)]
    [TestCase(FieldAttributes.InitOnly, ExpectedResult = false)]
    [TestCase(FieldAttributes.Public, ExpectedResult = false)]
    [TestCase(FieldAttributes.Literal, ExpectedResult = false)]
    [TestCase(FieldAttributes.Static, ExpectedResult = false)]
    [TestCase(FieldAttributes.NotSerialized, ExpectedResult = false)]
    [TestCase(FieldAttributes.Private, ExpectedResult = false)]
    [TestCase(FieldAttributes.PrivateScope, ExpectedResult = false)]
    [TestCase(FieldAttributes.SpecialName, ExpectedResult = false)]
    public bool Test_IsInternal_Works_Correctly(FieldAttributes attributes)
    {
        var m = new Mock<FieldInfo>();
        m.SetupGet(f => f.Attributes).Returns(attributes);

        return m.Object.IsInternal();
    }

    [Test]
    [TestCase(FieldAttributes.Assembly, ExpectedResult = true)]
    [TestCase(FieldAttributes.FamORAssem, ExpectedResult = true)]
    [TestCase(FieldAttributes.FamANDAssem, ExpectedResult = false)]
    [TestCase(FieldAttributes.Family, ExpectedResult = false)]
    [TestCase(FieldAttributes.InitOnly, ExpectedResult = false)]
    [TestCase(FieldAttributes.Public, ExpectedResult = true)]
    [TestCase(FieldAttributes.Literal, ExpectedResult = false)]
    [TestCase(FieldAttributes.Static, ExpectedResult = false)]
    [TestCase(FieldAttributes.NotSerialized, ExpectedResult = false)]
    [TestCase(FieldAttributes.Private, ExpectedResult = false)]
    [TestCase(FieldAttributes.PrivateScope, ExpectedResult = false)]
    [TestCase(FieldAttributes.SpecialName, ExpectedResult = false)]
    public bool Test_IsPublicOrInternal_Works_Correctly(FieldAttributes attributes)
    {
        var m = new Mock<FieldInfo>();
        m.SetupGet(f => f.Attributes).Returns(attributes);

        return m.Object.IsPublicOrInternal();
    }
}
