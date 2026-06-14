using BAUERGROUP.Shared.Core.Utilities;

namespace BAUERGROUP.Shared.Test.Core;

public class TestItem
{
    public string Value { get; set; } = string.Empty;
}

public class LockingListTests : IDisposable
{
    private readonly LockingList<TestItem> _sut;

    public LockingListTests()
    {
        _sut = new LockingList<TestItem>("TestCache");
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    [Fact]
    public void Add_NewItem_ReturnsTrue()
    {
        var result = _sut.Add("key1", new TestItem { Value = "value1" }, TimeSpan.FromMinutes(5));
        result.Should().BeTrue();
    }

    [Fact]
    public void Add_DuplicateKey_WithoutOverwrite_ReturnsFalse()
    {
        _sut.Add("key1", new TestItem { Value = "value1" }, TimeSpan.FromMinutes(5));
        var result = _sut.Add("key1", new TestItem { Value = "value2" }, TimeSpan.FromMinutes(5), overwrite: false);
        result.Should().BeFalse();
    }

    [Fact]
    public void Add_DuplicateKey_WithOverwrite_ReturnsTrue()
    {
        _sut.Add("key1", new TestItem { Value = "value1" }, TimeSpan.FromMinutes(5));
        var result = _sut.Add("key1", new TestItem { Value = "value2" }, TimeSpan.FromMinutes(5), overwrite: true);
        result.Should().BeTrue();
    }

    [Fact]
    public void Get_ExistingKey_ReturnsValue()
    {
        _sut.Add("key1", new TestItem { Value = "value1" }, TimeSpan.FromMinutes(5));
        var result = _sut.Get("key1");
        result.Should().NotBeNull();
        result!.Value.Should().Be("value1");
    }

    [Fact]
    public void Get_NonExistingKey_ReturnsNull()
    {
        var result = _sut.Get("nonexistent");
        result.Should().BeNull();
    }

    [Fact]
    public void IsExists_ExistingKey_ReturnsTrue()
    {
        _sut.Add("key1", new TestItem { Value = "value1" }, TimeSpan.FromMinutes(5));
        var result = _sut.IsExists("key1");
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExists_NonExistingKey_ReturnsFalse()
    {
        var result = _sut.IsExists("nonexistent");
        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_ExistingKey_ReturnsValue()
    {
        _sut.Add("key1", new TestItem { Value = "value1" }, TimeSpan.FromMinutes(5));
        var result = _sut.Remove("key1");
        result.Should().NotBeNull();
        result!.Value.Should().Be("value1");
    }

    [Fact]
    public void Remove_NonExistingKey_ReturnsDefault()
    {
        var result = _sut.Remove("nonexistent");
        result.Should().BeNull();
    }

    [Fact]
    public void Remove_DecreasesCount()
    {
        _sut.Add("key1", new TestItem { Value = "value1" }, TimeSpan.FromMinutes(5));
        _sut.Add("key2", new TestItem { Value = "value2" }, TimeSpan.FromMinutes(5));
        _sut.Remove("key1");
        _sut.Count.Should().Be(1);
    }

    [Fact]
    public void Count_ReturnsCorrectCount()
    {
        _sut.Add("key1", new TestItem { Value = "value1" }, TimeSpan.FromMinutes(5));
        _sut.Add("key2", new TestItem { Value = "value2" }, TimeSpan.FromMinutes(5));
        _sut.Add("key3", new TestItem { Value = "value3" }, TimeSpan.FromMinutes(5));

        _sut.Count.Should().Be(3);
    }

    [Fact]
    public void Updated_Event_FiresOnAdd()
    {
        var eventFired = false;
        _sut.Updated += (s, e) => eventFired = true;

        _sut.Add("key1", new TestItem { Value = "value1" }, TimeSpan.FromMinutes(5));

        eventFired.Should().BeTrue();
    }

    [Fact]
    public void Updated_Event_FiresOnRemove()
    {
        _sut.Add("key1", new TestItem { Value = "value1" }, TimeSpan.FromMinutes(5));

        var eventFired = false;
        _sut.Updated += (s, e) => eventFired = true;

        _sut.Remove("key1");

        eventFired.Should().BeTrue();
    }
}
