using BAUERGROUP.Shared.Cloud.CloudinaryClient;

namespace BAUERGROUP.Shared.Test.Cloud;

public class CloudinaryConfigurationTests
{
    [Fact]
    public void Constructor_WithParameters_SetsProperties()
    {
        var config = new CloudinaryImageManagerConfiguration(
            "TestCloud",
            "api-key-123",
            "api-secret-456",
            "my-project"
        );

        config.Name.Should().Be("TestCloud");
        config.APIKey.Should().Be("api-key-123");
        config.APISecret.Should().Be("api-secret-456");
        config.Project.Should().Be("my-project");
    }

    [Fact]
    public void Constructor_Default_HasEmptyStrings()
    {
        var config = new CloudinaryImageManagerConfiguration();

        config.Name.Should().BeEmpty();
        config.APIKey.Should().BeEmpty();
        config.APISecret.Should().BeEmpty();
        config.Project.Should().BeEmpty();
    }

    [Fact]
    public void Properties_CanBeModified()
    {
        var config = new CloudinaryImageManagerConfiguration();

        config.Name = "NewName";
        config.APIKey = "NewKey";
        config.APISecret = "NewSecret";
        config.Project = "NewProject";

        config.Name.Should().Be("NewName");
        config.APIKey.Should().Be("NewKey");
        config.APISecret.Should().Be("NewSecret");
        config.Project.Should().Be("NewProject");
    }
}
