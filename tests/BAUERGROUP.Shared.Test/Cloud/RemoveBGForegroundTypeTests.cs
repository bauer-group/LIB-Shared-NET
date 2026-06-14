using BAUERGROUP.Shared.Cloud.RemoveBG;

namespace BAUERGROUP.Shared.Test.Cloud;

public class RemoveBGForegroundTypeTests
{
    [Fact]
    public void Enum_HasAutoValue()
    {
        RemoveBGForegroundType.Auto.Should().BeDefined();
    }

    [Fact]
    public void Enum_HasPersonValue()
    {
        RemoveBGForegroundType.Person.Should().BeDefined();
    }

    [Fact]
    public void Enum_HasProductValue()
    {
        RemoveBGForegroundType.Product.Should().BeDefined();
    }

    [Fact]
    public void Enum_HasCarValue()
    {
        RemoveBGForegroundType.Car.Should().BeDefined();
    }

    [Fact]
    public void Enum_HasFourValues()
    {
        var values = Enum.GetValues<RemoveBGForegroundType>();
        values.Length.Should().Be(4);
    }

    [Theory]
    [InlineData(RemoveBGForegroundType.Auto, 0)]
    [InlineData(RemoveBGForegroundType.Person, 1)]
    [InlineData(RemoveBGForegroundType.Product, 2)]
    [InlineData(RemoveBGForegroundType.Car, 3)]
    public void Enum_HasExpectedIntValues(RemoveBGForegroundType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Fact]
    public void Enum_CanBeParsedFromString()
    {
        var parsed = Enum.Parse<RemoveBGForegroundType>("Person");
        parsed.Should().Be(RemoveBGForegroundType.Person);
    }
}
