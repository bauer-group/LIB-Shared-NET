using BAUERGROUP.Shared.Cloud.CloudinaryClient;

namespace BAUERGROUP.Shared.Test.Cloud;

public class CloudinaryImageFormatTests
{
    [Fact]
    public void Enum_HasOriginalValue()
    {
        CloudinaryImageFormat.Original.Should().BeDefined();
    }

    [Fact]
    public void Enum_HasPNGValue()
    {
        CloudinaryImageFormat.PNG.Should().BeDefined();
    }

    [Fact]
    public void Enum_HasJPEGValue()
    {
        CloudinaryImageFormat.JPEG.Should().BeDefined();
    }

    [Fact]
    public void Enum_HasThreeValues()
    {
        var values = Enum.GetValues<CloudinaryImageFormat>();
        values.Length.Should().Be(3);
    }

    [Theory]
    [InlineData(CloudinaryImageFormat.Original, 0)]
    [InlineData(CloudinaryImageFormat.PNG, 1)]
    [InlineData(CloudinaryImageFormat.JPEG, 2)]
    public void Enum_HasExpectedIntValues(CloudinaryImageFormat format, int expectedValue)
    {
        ((int)format).Should().Be(expectedValue);
    }

    [Fact]
    public void Enum_CanBeParsedFromString()
    {
        var parsed = Enum.Parse<CloudinaryImageFormat>("PNG");
        parsed.Should().Be(CloudinaryImageFormat.PNG);
    }

    [Fact]
    public void Enum_ToString_ReturnsCorrectString()
    {
        CloudinaryImageFormat.Original.ToString().Should().Be("Original");
        CloudinaryImageFormat.PNG.ToString().Should().Be("PNG");
        CloudinaryImageFormat.JPEG.ToString().Should().Be("JPEG");
    }
}
