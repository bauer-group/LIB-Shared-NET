using BAUERGROUP.Shared.Data.Application.Data;

namespace BAUERGROUP.Shared.Test.Data;

public class IApplicationDataStoreEntryTests
{
    [Fact]
    public void Interface_HasUIDProperty()
    {
        var entry = new TestDataStoreEntry();

        entry.UID = Guid.NewGuid();

        entry.UID.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void UID_CanBeSet()
    {
        var entry = new TestDataStoreEntry();
        var guid = Guid.NewGuid();

        entry.UID = guid;

        entry.UID.Should().Be(guid);
    }

    [Fact]
    public void UID_DefaultIsEmptyGuid()
    {
        var entry = new TestDataStoreEntry();

        entry.UID.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Implementation_CanBeUsedPolymorphically()
    {
        IApplicationDataStoreEntry entry = new TestDataStoreEntry { UID = Guid.NewGuid() };

        entry.UID.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void MultipleEntries_HaveDifferentUIDs()
    {
        var entry1 = new TestDataStoreEntry { UID = Guid.NewGuid() };
        var entry2 = new TestDataStoreEntry { UID = Guid.NewGuid() };

        entry1.UID.Should().NotBe(entry2.UID);
    }

    private class TestDataStoreEntry : IApplicationDataStoreEntry
    {
        public Guid UID { get; set; }
        public string? Name { get; set; }
        public int Value { get; set; }
    }
}
