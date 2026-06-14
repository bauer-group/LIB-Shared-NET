using BAUERGROUP.Shared.Data.EmbeddedDatabase;

namespace BAUERGROUP.Shared.Test.Data;

public class ConcurrentPersistentDictionaryTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ConcurrentPersistentDictionary<string, string> _sut;

    public ConcurrentPersistentDictionaryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"CPD_Test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _sut = new ConcurrentPersistentDictionary<string, string>(_testDirectory, "TestDB", "TestTable");
    }

    public void Dispose()
    {
        _sut.Dispose();
        if (Directory.Exists(_testDirectory))
        {
            try { Directory.Delete(_testDirectory, true); } catch { }
        }
    }

    [Fact]
    public void Create_NewEntry_CanBeRead()
    {
        _sut.Create("key1", "value1");
        var result = _sut.Read("key1");
        result.Should().Be("value1");
    }

    [Fact]
    public void Create_DuplicateKey_OverwritesValue()
    {
        _sut.Create("key1", "value1");
        _sut.Create("key1", "value2");
        var result = _sut.Read("key1");
        result.Should().Be("value2");
    }

    [Fact]
    public void Update_ExistingEntry_UpdatesValue()
    {
        _sut.Create("key1", "value1");
        _sut.Update("key1", "updated");
        var result = _sut.Read("key1");
        result.Should().Be("updated");
    }

    [Fact]
    public void Delete_ExistingEntry_RemovesIt()
    {
        _sut.Create("key1", "value1");
        _sut.Delete("key1");
        var result = _sut.Read("key1");
        result.Should().BeNull();
    }

    [Fact]
    public void Exists_ExistingKey_ReturnsTrue()
    {
        _sut.Create("key1", "value1");
        var result = _sut.Exists("key1");
        result.Should().BeTrue();
    }

    [Fact]
    public void Exists_NonExistingKey_ReturnsFalse()
    {
        var result = _sut.Exists("nonexistent");
        result.Should().BeFalse();
    }

    [Fact]
    public void Count_ReturnsCorrectCount()
    {
        _sut.Create("key1", "value1");
        _sut.Create("key2", "value2");
        _sut.Create("key3", "value3");
        _sut.Count.Should().Be(3);
    }

    [Fact]
    public void Read_AllEntries_ReturnsAllValues()
    {
        _sut.Create("key1", "value1");
        _sut.Create("key2", "value2");
        var results = _sut.Read();
        results.Should().HaveCount(2);
        results.Should().Contain("value1");
        results.Should().Contain("value2");
    }

    [Fact]
    public void ReadWithKeys_ReturnsCorrectDictionary()
    {
        _sut.Create("key1", "value1");
        _sut.Create("key2", "value2");
        var results = _sut.ReadWithKeys();
        results.Should().HaveCount(2);
        results["key1"].Should().Be("value1");
        results["key2"].Should().Be("value2");
    }

    [Fact]
    public void Read_NonExistingKey_ReturnsDefault()
    {
        var result = _sut.Read("nonexistent");
        result.Should().BeNull();
    }
}

public class ConcurrentPersistentDictionaryIntKeyTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ConcurrentPersistentDictionary<int, TestData> _sut;

    public ConcurrentPersistentDictionaryIntKeyTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"CPD_Test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _sut = new ConcurrentPersistentDictionary<int, TestData>(_testDirectory, "TestDB2", "TestTable2");
    }

    public void Dispose()
    {
        _sut.Dispose();
        if (Directory.Exists(_testDirectory))
        {
            try { Directory.Delete(_testDirectory, true); } catch { }
        }
    }

    [Fact]
    public void Create_WithComplexValue_CanBeRead()
    {
        var data = new TestData { Name = "Test", Value = 42 };
        _sut.Create(1, data);
        var result = _sut.Read(1);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Create_MultipleEntries_MaintainsIntegrity()
    {
        for (int i = 0; i < 10; i++)
        {
            _sut.Create(i, new TestData { Name = $"Item{i}", Value = i * 10 });
        }

        _sut.Count.Should().Be(10);

        for (int i = 0; i < 10; i++)
        {
            var result = _sut.Read(i);
            result.Should().NotBeNull();
            result!.Name.Should().Be($"Item{i}");
            result.Value.Should().Be(i * 10);
        }
    }

    public class TestData
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
