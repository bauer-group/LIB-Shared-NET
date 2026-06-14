using BAUERGROUP.Shared.Core.Extensions;
using System.Xml.Linq;

namespace BAUERGROUP.Shared.Test.Core;

public class LinqHelperTests
{
    [Fact]
    public void XMLString_WithValue_ReturnsValue()
    {
        var element = new XElement("test", "Hello World");
        var result = element.XMLString();

        result.Should().Be("Hello World");
    }

    [Fact]
    public void XMLString_WithNull_ReturnsEmptyString()
    {
        XElement? element = null;
        var result = element!.XMLString();

        result.Should().BeEmpty();
    }

    [Fact]
    public void XMLString_WithEmptyElement_ReturnsEmpty()
    {
        var element = new XElement("test");
        var result = element.XMLString();

        result.Should().BeEmpty();
    }

    [Fact]
    public void XMLDecimal_WithValue_ReturnsDecimal()
    {
        var element = new XElement("price", "12345");
        var result = element.XMLDecimal();

        result.Should().Be(12345m);
    }

    [Fact]
    public void XMLDecimal_WithNull_ReturnsZero()
    {
        XElement? element = null;
        var result = element!.XMLDecimal();

        result.Should().Be(0m);
    }

    [Fact]
    public void XMLDecimal_WithIntegerValue_ReturnsDecimal()
    {
        var element = new XElement("count", "100");
        var result = element.XMLDecimal();

        result.Should().Be(100m);
    }

    [Fact]
    public void PropertyName_ReturnsCorrectPropertyName()
    {
        var result = LinqHelper.PropertyName<TestClass>(x => x.Name);

        result.Should().Be("Name");
    }

    [Fact]
    public void PropertyName_WithIntProperty_ReturnsCorrectName()
    {
        var result = LinqHelper.PropertyName<TestClass>(x => x.Age);

        result.Should().Be("Age");
    }

    private class TestClass
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
