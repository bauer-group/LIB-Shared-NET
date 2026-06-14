using BAUERGROUP.Shared.Core.Extensions;
using System.Xml.Serialization;

namespace BAUERGROUP.Shared.Test.Core;

public class XMLHelperTests
{
    [Serializable]
    public class TestData
    {
        public string? Name { get; set; }
        public int Value { get; set; }
        public List<string>? Items { get; set; }
    }

    [Fact]
    public void SerializeToXML_WithValidObject_ReturnsXmlString()
    {
        var data = new TestData { Name = "Test", Value = 42 };

        var result = data.SerializeToXML();

        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Test");
        result.Should().Contain("42");
        result.Should().Contain("<?xml");
    }

    [Fact]
    public void SerializeToXML_WithNull_ReturnsNull()
    {
        TestData? data = null;

        var result = data.SerializeToXML();

        result.Should().BeNull();
    }

    [Fact]
    public void DeserializeFromXML_WithValidXml_ReturnsObject()
    {
        var original = new TestData { Name = "Test", Value = 123 };
        var xml = original.SerializeToXML();

        var result = xml!.DeserializeFromXML<TestData>();

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
        result.Value.Should().Be(123);
    }

    [Fact]
    public void DeserializeFromXML_WithNull_ReturnsDefault()
    {
        string? xml = null;

        var result = xml!.DeserializeFromXML<TestData>();

        result.Should().BeNull();
    }

    [Fact]
    public void DeserializeFromXML_WithEmptyString_ReturnsDefault()
    {
        var xml = string.Empty;

        var result = xml.DeserializeFromXML<TestData>();

        result.Should().BeNull();
    }

    [Fact]
    public void RoundTrip_PreservesComplexObject()
    {
        var original = new TestData
        {
            Name = "Complex Test",
            Value = 999,
            Items = ["Item1", "Item2", "Item3"]
        };

        var xml = original.SerializeToXML();
        var result = xml!.DeserializeFromXML<TestData>();

        result.Should().NotBeNull();
        result!.Name.Should().Be(original.Name);
        result.Value.Should().Be(original.Value);
        result.Items.Should().BeEquivalentTo(original.Items);
    }

    [Fact]
    public void XMLEscape_WithSpecialCharacters_EscapesCorrectly()
    {
        var input = "<tag>Test & \"quotes\" 'apostrophe'</tag>";

        var result = input.XMLEscape();

        result.Should().Contain("&lt;");
        result.Should().Contain("&gt;");
        result.Should().Contain("&amp;");
        result.Should().Contain("&quot;");
        result.Should().Contain("&apos;");
    }

    [Fact]
    public void XMLEscape_WithNormalText_ReturnsUnchanged()
    {
        var input = "Normal text without special characters";

        var result = input.XMLEscape();

        result.Should().Be(input);
    }

    [Fact]
    public void XMLEscape_WithNull_ReturnsNull()
    {
        string? input = null;

        var result = input?.XMLEscape();

        result.Should().BeNull();
    }

    [Fact]
    public void SerializeToXML_WithSpecialCharacters_HandlesCorrectly()
    {
        var data = new TestData { Name = "Test <with> special & chars", Value = 1 };

        var xml = data.SerializeToXML();
        var result = xml!.DeserializeFromXML<TestData>();

        result!.Name.Should().Be(data.Name);
    }
}
