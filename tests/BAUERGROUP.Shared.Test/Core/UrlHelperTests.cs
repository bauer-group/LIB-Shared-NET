using BAUERGROUP.Shared.Core.Extensions;

namespace BAUERGROUP.Shared.Test.Core;

public class UrlHelperTests
{
    public class UrlParameterData
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
    }

    [Fact]
    public void SetURLParameters_WithSimpleData_ReplacesParameters()
    {
        var data = new UrlParameterData { Id = "123", Name = "Test" };
        var url = "https://api.example.com/items/{Id}/details/{Name}";

        var result = data.SetURLParameters(url);

        result.Should().Be("https://api.example.com/items/123/details/Test");
    }

    [Fact]
    public void SetURLParameters_WithNullValue_ReplacesWithEmpty()
    {
        var data = new UrlParameterData { Id = "123", Name = null };
        var url = "https://api.example.com/items/{Id}?name={Name}";

        var result = data.SetURLParameters(url);

        result.Should().Be("https://api.example.com/items/123?name=");
    }

    [Fact]
    public void SetURLParameters_WithMultipleParameters_ReplacesAll()
    {
        var data = new UrlParameterData { Id = "1", Name = "Test", Category = "Electronics" };
        var url = "https://api.example.com/{Category}/items/{Id}?name={Name}";

        var result = data.SetURLParameters(url);

        result.Should().Be("https://api.example.com/Electronics/items/1?name=Test");
    }

    [Fact]
    public void SetURLParameters_WithNoMatchingPlaceholders_ReturnsUnchanged()
    {
        var data = new UrlParameterData { Id = "123" };
        var url = "https://api.example.com/items";

        var result = data.SetURLParameters(url);

        result.Should().Be("https://api.example.com/items");
    }

    [Fact]
    public void SetURLParameters_WithEmptyUrl_ReturnsEmpty()
    {
        var data = new UrlParameterData { Id = "123" };
        var url = "";

        var result = data.SetURLParameters(url);

        result.Should().BeEmpty();
    }

    [Fact]
    public void SetURLParameters_CaseSensitive_ReplacesExactMatch()
    {
        var data = new { ID = "123" };
        var url = "https://api.example.com/items/{ID}";

        var result = data.SetURLParameters(url);

        result.Should().Be("https://api.example.com/items/123");
    }

    [Fact]
    public void SetURLParameters_WithSpecialCharactersInValue_ReplacesCorrectly()
    {
        var data = new UrlParameterData { Name = "Test&Name" };
        var url = "https://api.example.com/items?name={Name}";

        var result = data.SetURLParameters(url);

        // URL encoding: & becomes %26
        result.Should().Contain("Test%26Name");
    }

    public class QueryData
    {
        public string? Search { get; set; }
        public string? Filter { get; set; }
    }

    [Fact]
    public void SetURLParameters_WithQueryStringFormat_ReplacesCorrectly()
    {
        var data = new QueryData { Search = "keyword", Filter = "active" };
        var url = "https://api.example.com/search?q={Search}&status={Filter}";

        var result = data.SetURLParameters(url);

        result.Should().Be("https://api.example.com/search?q=keyword&status=active");
    }
}
