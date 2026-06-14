using BAUERGROUP.Shared.Cloud.RemoveBG;

namespace BAUERGROUP.Shared.Test.Cloud;

public class RemoveBGImageFormatTests
{
    [Fact]
    public void Enum_HasAutoValue()
    {
        RemoveBGImageFormat.Auto.Should().BeDefined();
    }

    [Fact]
    public void Enum_HasPNGValue()
    {
        RemoveBGImageFormat.PNG.Should().BeDefined();
    }

    [Fact]
    public void Enum_HasJPGValue()
    {
        RemoveBGImageFormat.JPG.Should().BeDefined();
    }

    [Fact]
    public void Enum_HasZIPValue()
    {
        RemoveBGImageFormat.ZIP.Should().BeDefined();
    }

    [Fact]
    public void Enum_HasFourValues()
    {
        var values = Enum.GetValues<RemoveBGImageFormat>();
        values.Length.Should().Be(4);
    }

    [Theory]
    [InlineData(RemoveBGImageFormat.Auto, 0)]
    [InlineData(RemoveBGImageFormat.PNG, 1)]
    [InlineData(RemoveBGImageFormat.JPG, 2)]
    [InlineData(RemoveBGImageFormat.ZIP, 3)]
    public void Enum_HasExpectedIntValues(RemoveBGImageFormat format, int expectedValue)
    {
        ((int)format).Should().Be(expectedValue);
    }

    [Fact]
    public void Enum_CanBeParsedFromString()
    {
        var parsed = Enum.Parse<RemoveBGImageFormat>("PNG");
        parsed.Should().Be(RemoveBGImageFormat.PNG);
    }
}
