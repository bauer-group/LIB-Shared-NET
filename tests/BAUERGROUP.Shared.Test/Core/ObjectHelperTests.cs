using BAUERGROUP.Shared.Core.Extensions;

namespace BAUERGROUP.Shared.Test.Core;

public class ObjectHelperTests
{
    [Fact]
    public void TrimPublicStringProperties_TrimsWhitespace()
    {
        var obj = new TestPerson { Name = "  John  ", City = "  Berlin  " };
        obj.TrimPublicStringProperties();

        obj.Name.Should().Be("John");
        obj.City.Should().Be("Berlin");
    }

    [Fact]
    public void TrimPublicStringProperties_NullBecomesEmpty()
    {
        var obj = new TestPerson { Name = null!, City = "Test" };
        obj.TrimPublicStringProperties();

        obj.Name.Should().BeEmpty();
    }

    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var original = new TestPerson { Name = "John", Age = 30, City = "Berlin" };
        var clone = original.Clone();

        clone.Should().NotBeNull();
        clone!.Name.Should().Be("John");
        clone.Age.Should().Be(30);
        clone.City.Should().Be("Berlin");
    }

    [Fact]
    public void Clone_ModifyingCloneDoesNotAffectOriginal()
    {
        var original = new TestPerson { Name = "John", Age = 30 };
        var clone = original.Clone();

        clone!.Name = "Jane";
        clone.Age = 25;

        original.Name.Should().Be("John");
        original.Age.Should().Be(30);
    }

    [Fact]
    public void Clone_WithNull_ReturnsDefault()
    {
        TestPerson? original = null;
        var clone = original.Clone();

        clone.Should().BeNull();
    }

    [Fact]
    public void SerializeToJSON_CreatesValidJson()
    {
        var obj = new TestPerson { Name = "John", Age = 30 };
        var json = obj.SerializeToJSON();

        json.Should().Contain("John");
        json.Should().Contain("30");
    }

    [Fact]
    public void SerializeToJSON_WithIndentation_IsFormatted()
    {
        var obj = new TestPerson { Name = "John", Age = 30 };
        var json = obj.SerializeToJSON(indented: true);

        json.Should().Contain("\n");
    }

    [Fact]
    public void SerializeToJSON_WithoutIndentation_IsCompact()
    {
        var obj = new TestPerson { Name = "John", Age = 30 };
        var json = obj.SerializeToJSON(indented: false);

        json.Should().NotContain("\n");
    }

    [Fact]
    public void DeserializeFromJSON_CreatesObject()
    {
        var json = "{\"Name\":\"John\",\"Age\":30,\"City\":\"Berlin\"}";
        var obj = json.DeserializeFromJSON<TestPerson>();

        obj.Should().NotBeNull();
        obj!.Name.Should().Be("John");
        obj.Age.Should().Be(30);
    }

    [Fact]
    public void SerializeAndDeserialize_RoundTrips()
    {
        var original = new TestPerson { Name = "John", Age = 30, City = "Berlin" };
        var json = original.SerializeToJSON();
        var restored = json.DeserializeFromJSON<TestPerson>();

        restored.Should().NotBeNull();
        restored!.Name.Should().Be(original.Name);
        restored.Age.Should().Be(original.Age);
        restored.City.Should().Be(original.City);
    }

    [Fact]
    public void PropertiesEqual_SameValues_ReturnsTrue()
    {
        var obj1 = new TestPerson { Name = "John", Age = 30 };
        var obj2 = new TestPerson { Name = "John", Age = 30 };

        obj1.PropertiesEqual(obj2).Should().BeTrue();
    }

    [Fact]
    public void PropertiesEqual_DifferentValues_ReturnsFalse()
    {
        var obj1 = new TestPerson { Name = "John", Age = 30 };
        var obj2 = new TestPerson { Name = "Jane", Age = 25 };

        obj1.PropertiesEqual(obj2).Should().BeFalse();
    }

    [Fact]
    public void PropertiesEqual_BothNull_ReturnsTrue()
    {
        TestPerson? obj1 = null;
        TestPerson? obj2 = null;

        obj1.PropertiesEqual(obj2).Should().BeTrue();
    }

    [Fact]
    public void PropertiesEqual_OneNull_ReturnsFalse()
    {
        var obj1 = new TestPerson { Name = "John" };
        TestPerson? obj2 = null;

        obj1.PropertiesEqual(obj2).Should().BeFalse();
    }

    [Fact]
    public void GetPropertyInformations_ReturnsAllPublicProperties()
    {
        var obj = new TestPerson();
        var properties = obj.GetPropertyInformations().ToList();

        properties.Should().HaveCount(3);
        properties.Select(p => p.Name).Should().Contain("Name");
        properties.Select(p => p.Name).Should().Contain("Age");
        properties.Select(p => p.Name).Should().Contain("City");
    }

    [Fact]
    public void GetPropertyInformations_WithNull_ReturnsEmpty()
    {
        TestPerson? obj = null;
        var properties = obj.GetPropertyInformations();

        properties.Should().BeEmpty();
    }

    [Fact]
    public void ConvertTo_ConvertsCompatibleTypes()
    {
        var source = new TestPerson { Name = "John", Age = 30, City = "Berlin" };
        var target = source.ConvertTo<TestPersonDto, TestPerson>();

        target.Should().NotBeNull();
        target!.Name.Should().Be("John");
        target.Age.Should().Be(30);
    }

    public class TestPerson
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string City { get; set; } = string.Empty;
    }

    public class TestPersonDto
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
