using BAUERGROUP.Shared.Data.EmbeddedDatabase;
using NMemory.Tables;

namespace BAUERGROUP.Shared.Test.Data;

public class InMemoryRelationalDatabaseTests
{
    [Fact]
    public void Constructor_CreatesEmptyDatabase()
    {
        var db = new InMemoryRelationalDatabase();

        db.Should().NotBeNull();
    }

    [Fact]
    public void Database_DerivesFromNMemoryDatabase()
    {
        var db = new InMemoryRelationalDatabase();

        db.Should().BeAssignableTo<NMemory.Database>();
    }

    [Fact]
    public void Tables_CanBeCreated()
    {
        var db = new TestDatabase();

        db.TestEntities.Should().NotBeNull();
    }

    [Fact]
    public void Insert_AddsEntityToTable()
    {
        var db = new TestDatabase();
        var entity = new TestEntity { Id = 1, Name = "Test" };

        db.TestEntities.Insert(entity);

        db.TestEntities.Count.Should().Be(1);
    }

    [Fact]
    public void Select_RetrievesEntity()
    {
        var db = new TestDatabase();
        var entity = new TestEntity { Id = 1, Name = "Test" };
        db.TestEntities.Insert(entity);

        var retrieved = db.TestEntities.FirstOrDefault(e => e.Id == 1);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test");
    }

    [Fact]
    public void Delete_RemovesEntity()
    {
        var db = new TestDatabase();
        var entity = new TestEntity { Id = 1, Name = "Test" };
        db.TestEntities.Insert(entity);

        db.TestEntities.Delete(entity);

        db.TestEntities.Count.Should().Be(0);
    }

    [Fact]
    public void Update_ModifiesEntity()
    {
        var db = new TestDatabase();
        var entity = new TestEntity { Id = 1, Name = "Original" };
        db.TestEntities.Insert(entity);

        entity.Name = "Updated";
        db.TestEntities.Update(entity);

        var retrieved = db.TestEntities.FirstOrDefault(e => e.Id == 1);
        retrieved!.Name.Should().Be("Updated");
    }

    [Fact]
    public void MultipleInserts_IncreasesCount()
    {
        var db = new TestDatabase();

        db.TestEntities.Insert(new TestEntity { Id = 1, Name = "First" });
        db.TestEntities.Insert(new TestEntity { Id = 2, Name = "Second" });
        db.TestEntities.Insert(new TestEntity { Id = 3, Name = "Third" });

        db.TestEntities.Count.Should().Be(3);
    }

    [Fact]
    public void Query_WithPredicate_FiltersResults()
    {
        var db = new TestDatabase();
        db.TestEntities.Insert(new TestEntity { Id = 1, Name = "Active", IsActive = true });
        db.TestEntities.Insert(new TestEntity { Id = 2, Name = "Inactive", IsActive = false });
        db.TestEntities.Insert(new TestEntity { Id = 3, Name = "Active2", IsActive = true });

        var activeEntities = db.TestEntities.Where(e => e.IsActive).ToList();

        activeEntities.Should().HaveCount(2);
    }

    private class TestDatabase : InMemoryRelationalDatabase
    {
        public ITable<TestEntity> TestEntities { get; }

        public TestDatabase()
        {
            TestEntities = Tables.Create<TestEntity, int>(e => e.Id);
        }
    }

    private class TestEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }
    }
}
