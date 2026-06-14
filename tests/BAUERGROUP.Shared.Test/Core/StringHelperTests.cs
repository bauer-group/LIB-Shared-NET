using BAUERGROUP.Shared.Core.Extensions;
using System.Text;

namespace BAUERGROUP.Shared.Test.Core;

public class StringHelperTests
{
    [Fact]
    public void ReturnEmptyIfNull_WithNull_ReturnsEmptyString()
    {
        string? input = null;
        var result = input!.ReturnEmptyIfNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ReturnEmptyIfNull_WithValue_ReturnsValue()
    {
        var input = "test";
        var result = input.ReturnEmptyIfNull();
        result.Should().Be("test");
    }

    [Theory]
    [InlineData("000123", "123")]
    [InlineData("0001", "1")]
    [InlineData("abc", "abc")]
    [InlineData("", "")]
    public void RemoveLeadingZeros_ReturnsExpected(string input, string expected)
    {
        var result = input.RemoveLeadingZeros();
        result.Should().Be(expected);
    }

    [Fact]
    public void EncodeToHtml_EncodesSpecialCharacters()
    {
        var input = "<script>alert('test')</script>";
        var result = input.EncodeToHtml();
        result.Should().Contain("&lt;").And.Contain("&gt;");
    }

    [Fact]
    public void DecodeFromHtml_DecodesSpecialCharacters()
    {
        var input = "&lt;div&gt;";
        var result = input.DecodeFromHtml();
        result.Should().Be("<div>");
    }

    [Fact]
    public void BinaryToString_ConvertsCorrectly()
    {
        var encoding = Encoding.UTF8;
        var bytes = encoding.GetBytes("Hello");
        var result = bytes.BinaryToString(encoding);
        result.Should().Be("Hello");
    }

    [Fact]
    public void StringToBinary_ConvertsCorrectly()
    {
        var encoding = Encoding.UTF8;
        var input = "Hello";
        var result = input.StringToBinary(encoding);
        result.Should().BeEquivalentTo(encoding.GetBytes("Hello"));
    }

    [Theory]
    [InlineData("Hello World", "World", StringComparison.Ordinal, true)]
    [InlineData("Hello World", "world", StringComparison.Ordinal, false)]
    [InlineData("Hello World", "world", StringComparison.OrdinalIgnoreCase, true)]
    public void Contains_WithComparison_ReturnsExpected(string input, string compare, StringComparison comparison, bool expected)
    {
        var result = input.Contains(compare, comparison);
        result.Should().Be(expected);
    }

    [Fact]
    public void RemoveDelimiter_RemovesDelimiter()
    {
        var result = "a,b,c".RemoveDelimiter(",");
        result.Should().Be("abc");
    }

    [Theory]
    [InlineData("Hello World", 5, false, "Hello")]
    [InlineData("Hi", 10, false, "Hi")]
    [InlineData(null, 5, false, null)]
    [InlineData("", 5, false, "")]
    public void Truncate_ReturnsExpected(string? input, short maxLength, bool keepWholeWords, string? expected)
    {
        var result = input?.Truncate(maxLength, keepWholeWords);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Hello World Test", 3)]
    [InlineData("One", 1)]
    [InlineData("", 0)]
    [InlineData("   ", 0)]
    public void WordsCount_ReturnsExpected(string input, int expected)
    {
        var result = input.WordsCount();
        result.Should().Be(expected);
    }

    [Fact]
    public void HTML2Text_RemovesHtmlTags()
    {
        var input = "<p>Hello <b>World</b></p>";
        var result = input.HTML2Text();
        result.Should().Be("Hello World");
    }

    [Fact]
    public void HasNullOrWhiteSpaceValue_WithEmpty_ReturnsDefault()
    {
        var result = "".HasNullOrWhiteSpaceValue("default");
        result.Should().Be("default");
    }

    [Fact]
    public void HasNullOrWhiteSpaceValue_WithValue_ReturnsValue()
    {
        var result = "test".HasNullOrWhiteSpaceValue("default");
        result.Should().Be("test");
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData(null, true)]
    [InlineData("test", false)]
    public void IsNullOrWhiteSpace_ReturnsExpected(string? input, bool expected)
    {
        var result = input!.IsNullOrWhiteSpace();
        result.Should().Be(expected);
    }

    [Fact]
    public void RemoveLineBreaks_RemovesLineBreaks()
    {
        var input = "Hello\r\nWorld\tTest";
        var result = input.RemoveLineBreaks();
        result.Should().Be("HelloWorldTest");
    }

    [Fact]
    public void JoinWithFormat_JoinsCorrectly()
    {
        var list = new[] { 1, 2, 3 };
        var result = list.JoinWithFormat(", ", "[{0}]", false);
        result.Should().Be("[1], [2], [3]");
    }

    [Fact]
    public void SetParameterValue_ReplacesParameter()
    {
        var template = "Hello {name}!";
        var result = template.SetParameterValue("{name}", "World");
        result.Should().Be("Hello World!");
    }
}
