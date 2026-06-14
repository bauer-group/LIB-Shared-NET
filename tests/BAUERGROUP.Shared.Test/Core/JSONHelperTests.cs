#pragma warning disable CS0618 // Type or member is obsolete (JSONHelper is marked obsolete)

using BAUERGROUP.Shared.Core.Extensions;

namespace BAUERGROUP.Shared.Test.Core;

public class JSONHelperTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly List<string> _testFiles = [];

    public JSONHelperTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"JSONHelperTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        foreach (var file in _testFiles)
        {
            if (File.Exists(file))
                File.Delete(file);
        }
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }

    public class TestObject
    {
        public string? Name { get; set; }
        public int Value { get; set; }
        public TestEnum Status { get; set; }
    }

    public enum TestEnum
    {
        Active,
        Inactive,
        Pending
    }

    [Fact]
    public void Serialize_WithValidObject_ReturnsJson()
    {
        var obj = new TestObject { Name = "Test", Value = 42, Status = TestEnum.Active };

        var result = JSONHelper.Serialize(obj);

        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Test");
        result.Should().Contain("42");
    }

    [Fact]
    public void Deserialize_WithValidJson_ReturnsObject()
    {
        var json = """{"Name":"Test","Value":100,"Status":"Pending"}""";

        var result = JSONHelper.Deserialize<TestObject>(json);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
        result.Value.Should().Be(100);
        result.Status.Should().Be(TestEnum.Pending);
    }

    [Fact]
    public void Deserialize_IsCaseInsensitive()
    {
        var json = """{"NAME":"Test","VALUE":50}""";

        var result = JSONHelper.Deserialize<TestObject>(json);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
        result.Value.Should().Be(50);
    }

    [Fact]
    public void Serialize_EnumAsString()
    {
        var obj = new TestObject { Name = "EnumTest", Status = TestEnum.Inactive };

        var result = JSONHelper.Serialize(obj);

        result.Should().Contain("Inactive");
    }

    [Fact]
    public void RoundTrip_PreservesData()
    {
        var original = new TestObject { Name = "RoundTrip", Value = 999, Status = TestEnum.Pending };

        var json = JSONHelper.Serialize(original);
        var result = JSONHelper.Deserialize<TestObject>(json);

        result.Should().NotBeNull();
        result!.Name.Should().Be(original.Name);
        result.Value.Should().Be(original.Value);
        result.Status.Should().Be(original.Status);
    }

    [Fact]
    public void Serialize_ToStream_WritesCorrectly()
    {
        var obj = new TestObject { Name = "StreamTest", Value = 123 };
        var filePath = Path.Combine(_testDirectory, "stream_test.json");
        _testFiles.Add(filePath);

        using (var stream = File.Create(filePath))
        {
            JSONHelper.Serialize(obj, stream);
        }

        File.Exists(filePath).Should().BeTrue();
        var content = File.ReadAllText(filePath);
        content.Should().Contain("StreamTest");
    }

    [Fact]
    public void Deserialize_FromStream_ReadsCorrectly()
    {
        var filePath = Path.Combine(_testDirectory, "stream_read_test.json");
        _testFiles.Add(filePath);
        File.WriteAllText(filePath, """{"Name":"FromStream","Value":456}""");

        TestObject? result;
        using (var stream = File.OpenRead(filePath))
        {
            result = JSONHelper.Deserialize<TestObject>(stream);
        }

        result.Should().NotBeNull();
        result!.Name.Should().Be("FromStream");
        result.Value.Should().Be(456);
    }

    [Fact]
    public void Serialize_IsIndented()
    {
        var obj = new TestObject { Name = "Indented", Value = 1 };

        var result = JSONHelper.Serialize(obj);

        result.Should().Contain("\n");
    }
}
