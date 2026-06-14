using BAUERGROUP.Shared.Cloud.RemoveBG;

namespace BAUERGROUP.Shared.Test.Cloud;

public class RemoveBGClientOptionsTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        var options = new RemoveBGClientOptions();

        options.IsPreview.Should().BeFalse();
        options.Type.Should().Be(RemoveBGForegroundType.Auto);
        options.Format.Should().Be(RemoveBGImageFormat.Auto);
        options.BackgroundColor.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithIsPreview_SetsValue()
    {
        var options = new RemoveBGClientOptions(IsPreview: true);

        options.IsPreview.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithType_SetsValue()
    {
        var options = new RemoveBGClientOptions(Type: RemoveBGForegroundType.Person);

        options.Type.Should().Be(RemoveBGForegroundType.Person);
    }

    [Fact]
    public void Constructor_WithFormat_SetsValue()
    {
        var options = new RemoveBGClientOptions(Format: RemoveBGImageFormat.PNG);

        options.Format.Should().Be(RemoveBGImageFormat.PNG);
    }

    [Fact]
    public void Constructor_WithBackgroundColor_SetsValue()
    {
        var options = new RemoveBGClientOptions(BackgroundColor: "ff0000");

        options.BackgroundColor.Should().Be("ff0000");
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsAllValues()
    {
        var options = new RemoveBGClientOptions(
            IsPreview: true,
            Type: RemoveBGForegroundType.Car,
            Format: RemoveBGImageFormat.JPG,
            BackgroundColor: "00ff00");

        options.IsPreview.Should().BeTrue();
        options.Type.Should().Be(RemoveBGForegroundType.Car);
        options.Format.Should().Be(RemoveBGImageFormat.JPG);
        options.BackgroundColor.Should().Be("00ff00");
    }

    [Fact]
    public void Record_SupportsWithExpression()
    {
        var options = new RemoveBGClientOptions();

        var newOptions = options with { IsPreview = true };

        newOptions.IsPreview.Should().BeTrue();
        options.IsPreview.Should().BeFalse();
    }
}
