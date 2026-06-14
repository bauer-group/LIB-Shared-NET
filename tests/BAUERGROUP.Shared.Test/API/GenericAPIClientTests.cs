using BAUERGROUP.Shared.API.API;

namespace BAUERGROUP.Shared.Test.API;

public class GenericAPIClientTests
{
    [Fact]
    public void Constructor_WithValidUrl_CreatesClient()
    {
        using var client = new GenericAPIClient("https://api.example.com");

        client.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithInvalidUrl_ThrowsException()
    {
        var action = () => new GenericAPIClient("not-a-valid-url");

        action.Should().Throw<GenericAPIClientException>();
    }

    [Fact]
    public void Constructor_WithEmptyUrl_ThrowsException()
    {
        var action = () => new GenericAPIClient("");

        action.Should().Throw<GenericAPIClientException>();
    }

    [Fact]
    public void Constructor_WithRelativeUrl_ThrowsException()
    {
        var action = () => new GenericAPIClient("/api/items");

        action.Should().Throw<GenericAPIClientException>();
    }

    [Fact]
    public void Constructor_WithHttpUrl_CreatesClient()
    {
        using var client = new GenericAPIClient("http://api.example.com");

        client.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithCustomTimeout_CreatesClient()
    {
        using var client = new GenericAPIClient("https://api.example.com", 30000);

        client.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithZeroTimeout_UsesDefault()
    {
        using var client = new GenericAPIClient("https://api.example.com", 0);

        client.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNegativeTimeout_UsesDefault()
    {
        using var client = new GenericAPIClient("https://api.example.com", -1);

        client.Should().NotBeNull();
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var client = new GenericAPIClient("https://api.example.com");

        var action = () => client.Dispose();

        action.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var client = new GenericAPIClient("https://api.example.com");

        client.Dispose();
        var action = () => client.Dispose();

        action.Should().NotThrow();
    }

    [Fact]
    public void ResourceParameterProcessor_WithNullData_ReturnsOriginalResource()
    {
        using var client = new GenericAPIClient("https://api.example.com");

        var result = client.ResourceParameterProcessor<object>("/api/items", null);

        result.Should().Be("/api/items");
    }

    [Fact]
    public void ResourceParameterProcessor_WithData_ReplacesParameters()
    {
        using var client = new GenericAPIClient("https://api.example.com");
        var data = new { Id = "123", Name = "Test" };

        var result = client.ResourceParameterProcessor("/api/items/{Id}/{Name}", data);

        result.Should().Be("/api/items/123/Test");
    }

    [Fact]
    public void ResourceParameterProcessor_WithNoPlaceholders_ReturnsOriginal()
    {
        using var client = new GenericAPIClient("https://api.example.com");
        var data = new { Id = "123" };

        var result = client.ResourceParameterProcessor("/api/items", data);

        result.Should().Be("/api/items");
    }

    public class UrlTestData
    {
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
    }

    [Fact]
    public void ResourceParameterProcessor_WithComplexData_ReplacesAllParameters()
    {
        using var client = new GenericAPIClient("https://api.example.com");
        var data = new UrlTestData { Category = "Electronics", SubCategory = "Phones" };

        var result = client.ResourceParameterProcessor("/api/{Category}/{SubCategory}/items", data);

        result.Should().Be("/api/Electronics/Phones/items");
    }
}
