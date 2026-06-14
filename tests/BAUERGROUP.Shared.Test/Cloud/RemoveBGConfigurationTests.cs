using BAUERGROUP.Shared.Cloud.RemoveBG;

namespace BAUERGROUP.Shared.Test.Cloud;

public class RemoveBGConfigurationTests
{
    [Fact]
    public void Constructor_WithApiKey_SetsApiKey()
    {
        var config = new RemoveBGConfiguration("test-api-key");

        config.ApiKey.Should().Be("test-api-key");
    }

    [Fact]
    public void DefaultConstructor_SetsEmptyApiKey()
    {
        var config = new RemoveBGConfiguration();

        config.ApiKey.Should().BeEmpty();
    }

    [Fact]
    public void Record_SupportsWithExpression()
    {
        var config = new RemoveBGConfiguration("original-key");

        var newConfig = config with { ApiKey = "new-key" };

        newConfig.ApiKey.Should().Be("new-key");
        config.ApiKey.Should().Be("original-key");
    }

    [Fact]
    public void Record_SupportsEquality()
    {
        var config1 = new RemoveBGConfiguration("same-key");
        var config2 = new RemoveBGConfiguration("same-key");

        config1.Should().Be(config2);
    }

    [Fact]
    public void Record_SupportsInequality()
    {
        var config1 = new RemoveBGConfiguration("key1");
        var config2 = new RemoveBGConfiguration("key2");

        config1.Should().NotBe(config2);
    }
}
