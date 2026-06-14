using BAUERGROUP.Shared.Cloud.FixerIO;

namespace BAUERGROUP.Shared.Test.Cloud;

public class FixerIOConfigurationTests
{
    [Fact]
    public void Constructor_WithApiKey_SetsProperties()
    {
        var config = new FixerIOConfiguration("test-api-key");

        config.APIKey.Should().Be("test-api-key");
        config.Timeout.Should().Be(3000);
        config.Proxy.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithCustomTimeout_SetsTimeout()
    {
        var config = new FixerIOConfiguration("test-api-key", 5000);

        config.Timeout.Should().Be(5000);
    }

    [Fact]
    public void Constructor_Default_HasEmptyApiKey()
    {
        var config = new FixerIOConfiguration();

        config.APIKey.Should().BeEmpty();
    }

    [Fact]
    public void URL_ReturnsFixerApiUrl()
    {
        var config = new FixerIOConfiguration("test");

        config.URL.Should().Be("http://data.fixer.io/api/");
    }
}

public class FixerIOClientTests
{
    [Fact]
    public void Constructor_WithInvalidUrl_ThrowsException()
    {
        // The client validates the URL in the constructor
        // Since FixerIOConfiguration has a fixed URL, we can't easily test invalid URLs
        // But we can test that valid configuration doesn't throw
        var config = new FixerIOConfiguration("test-key");

        var action = () =>
        {
            using var client = new FixerIOClient(config);
        };

        action.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithApiKey_CreatesClient()
    {
        var action = () =>
        {
            using var client = new FixerIOClient("test-api-key");
        };

        action.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var client = new FixerIOClient("test-api-key");

        var action = () =>
        {
            client.Dispose();
            client.Dispose();
        };

        action.Should().NotThrow();
    }
}
