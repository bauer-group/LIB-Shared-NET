using BAUERGROUP.Shared.Cloud.CloudinaryClient;

namespace BAUERGROUP.Shared.Test.Cloud;

public class CloudinaryContentTypeTests
{
    [Fact]
    public void Enum_HasImageValue()
    {
        CloudinaryContentType.Image.Should().BeDefined();
    }

    [Fact]
    public void Enum_HasVideoValue()
    {
        CloudinaryContentType.Video.Should().BeDefined();
    }

    [Fact]
    public void Enum_HasTwoValues()
    {
        var values = Enum.GetValues<CloudinaryContentType>();
        values.Length.Should().Be(2);
    }

    [Theory]
    [InlineData(CloudinaryContentType.Image, 0)]
    [InlineData(CloudinaryContentType.Video, 1)]
    public void Enum_HasExpectedIntValues(CloudinaryContentType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Fact]
    public void Enum_CanBeParsedFromString()
    {
        var parsed = Enum.Parse<CloudinaryContentType>("Image");
        parsed.Should().Be(CloudinaryContentType.Image);
    }

    [Fact]
    public void Enum_ToString_ReturnsCorrectString()
    {
        CloudinaryContentType.Image.ToString().Should().Be("Image");
        CloudinaryContentType.Video.ToString().Should().Be("Video");
    }
}
