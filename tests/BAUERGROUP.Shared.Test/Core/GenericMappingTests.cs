using BAUERGROUP.Shared.Core.Utilities;

namespace BAUERGROUP.Shared.Test.Core;

public class GenericMappingTests
{
    [Fact]
    public void GetStringMappingDefinition_WithValidInput_ReturnsDictionary()
    {
        var input = new List<string> { "key1,value1", "key2,value2" };

        var result = GenericMapping.GetStringMappingDefinition(input);

        result.Should().HaveCount(2);
        result["key1"].Should().Be("value1");
        result["key2"].Should().Be("value2");
    }

    [Fact]
    public void GetStringMappingDefinition_WithCustomSeparator_ParsesCorrectly()
    {
        var input = new List<string> { "key1|value1", "key2|value2" };

        var result = GenericMapping.GetStringMappingDefinition(input, '|');

        result.Should().HaveCount(2);
        result["key1"].Should().Be("value1");
        result["key2"].Should().Be("value2");
    }

    [Fact]
    public void GetStringMappingDefinition_WithWhitespace_TrimsValues()
    {
        var input = new List<string> { "  key1  ,  value1  " };

        var result = GenericMapping.GetStringMappingDefinition(input);

        result["key1"].Should().Be("value1");
    }

    [Fact]
    public void GetStringMappingDefinition_WithEmptyStrings_SkipsEmpty()
    {
        var input = new List<string> { "key1,value1", "", "   ", "key2,value2" };

        var result = GenericMapping.GetStringMappingDefinition(input);

        result.Should().HaveCount(2);
    }

    [Fact]
    public void GetStringMappingDefinition_WithEmptyKey_SkipsWhenRemoveEmptyKeysTrue()
    {
        var input = new List<string> { ",value1", "key2,value2" };

        var result = GenericMapping.GetStringMappingDefinition(input, ',', true);

        result.Should().HaveCount(1);
        result.ContainsKey("").Should().BeFalse();
    }

    [Fact]
    public void GetStringMappingDefinition_WithEmptyKey_IncludesWhenRemoveEmptyKeysFalse()
    {
        var input = new List<string> { ",value1" };

        var result = GenericMapping.GetStringMappingDefinition(input, ',', false);

        result.Should().ContainKey("");
    }

    [Fact]
    public void GetStringMappingDefinition_WithInvalidFormat_ThrowsFormatException()
    {
        var input = new List<string> { "invalid_no_separator" };

        var action = () => GenericMapping.GetStringMappingDefinition(input);

        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void GetStringMappingDefinition_WithTooManySeparators_ThrowsFormatException()
    {
        var input = new List<string> { "key,value,extra" };

        var action = () => GenericMapping.GetStringMappingDefinition(input);

        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void ReplaceFieldContent_WithMatchingKey_ReturnsReplacement()
    {
        var mapping = new Dictionary<string, string> { { "old", "new" } };

        var result = GenericMapping.ReplaceFieldContent(mapping, "old");

        result.Should().Be("new");
    }

    [Fact]
    public void ReplaceFieldContent_IsCaseInsensitive()
    {
        var mapping = new Dictionary<string, string> { { "KEY", "value" } };

        var result = GenericMapping.ReplaceFieldContent(mapping, "key");

        result.Should().Be("value");
    }

    [Fact]
    public void ReplaceFieldContent_WithNoMatch_ReturnsOriginal()
    {
        var mapping = new Dictionary<string, string> { { "key", "value" } };

        var result = GenericMapping.ReplaceFieldContent(mapping, "other");

        result.Should().Be("other");
    }

    [Fact]
    public void ReplaceFieldContent_WithEmptyDictionary_ReturnsOriginal()
    {
        var mapping = new Dictionary<string, string>();

        var result = GenericMapping.ReplaceFieldContent(mapping, "input");

        result.Should().Be("input");
    }

    [Fact]
    public void GetIntMappingDefinition_WithValidInput_ReturnsDictionary()
    {
        var input = new List<string> { "1,value1", "2,value2" };

        var result = GenericMapping.GetIntMappingDefinition(input);

        result.Should().HaveCount(2);
        result[1].Should().Be("value1");
        result[2].Should().Be("value2");
    }

    [Fact]
    public void GetIntMappingDefinition_WithNonIntegerKey_ThrowsFormatException()
    {
        var input = new List<string> { "notanumber,value" };

        var action = () => GenericMapping.GetIntMappingDefinition(input);

        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void GetMultipleIntMappingDefinition_AllowsDuplicateKeys()
    {
        var input = new List<string> { "1,value1", "1,value2" };

        var result = GenericMapping.GetMultipleIntMappingDefinition(input);

        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Item1 == 1 && t.Item2 == "value1");
        result.Should().Contain(t => t.Item1 == 1 && t.Item2 == "value2");
    }

    [Fact]
    public void GetMultipleStringMappingDefinition_AllowsDuplicateKeys()
    {
        var input = new List<string> { "key,value1", "key,value2" };

        var result = GenericMapping.GetMultipleStringMappingDefinition(input);

        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Item1 == "key" && t.Item2 == "value1");
        result.Should().Contain(t => t.Item1 == "key" && t.Item2 == "value2");
    }

    [Fact]
    public void GetMultipleStringMappingDefinition_WithEmptyKey_SkipsWhenRemoveEmptyKeysTrue()
    {
        var input = new List<string> { ",value1", "key,value2" };

        var result = GenericMapping.GetMultipleStringMappingDefinition(input, ',', true);

        result.Should().HaveCount(1);
    }
}
