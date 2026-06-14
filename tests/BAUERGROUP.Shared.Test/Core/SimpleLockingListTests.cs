using BAUERGROUP.Shared.Core.Utilities;

namespace BAUERGROUP.Shared.Test.Core;

public class SimpleLockingListTests
{
    [Fact]
    public void Add_WithExpiration_AddsEntry()
    {
        var list = new SimpleLockingList("TestList");
        var expiration = TimeSpan.FromMinutes(5);

        list.Add("CODE123", expiration);

        list.IsExists("CODE123").Should().BeTrue();
    }

    [Fact]
    public void GetUnlockTime_ReturnsCorrectTime()
    {
        var list = new SimpleLockingList("TestList");
        var expiration = TimeSpan.FromHours(1);
        var beforeAdd = DateTime.UtcNow;

        list.Add("CODE123", expiration);
        var unlockTime = list.GetUnlockTime("CODE123");

        unlockTime.Should().BeAfter(beforeAdd);
        unlockTime.Should().BeCloseTo(DateTime.UtcNow.Add(expiration), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Add_MultipleEntries_AllAccessible()
    {
        var list = new SimpleLockingList("TestList");

        list.Add("CODE1", TimeSpan.FromMinutes(1));
        list.Add("CODE2", TimeSpan.FromMinutes(2));
        list.Add("CODE3", TimeSpan.FromMinutes(3));

        list.IsExists("CODE1").Should().BeTrue();
        list.IsExists("CODE2").Should().BeTrue();
        list.IsExists("CODE3").Should().BeTrue();
    }

    [Fact]
    public void IsExists_NonExistentCode_ReturnsFalse()
    {
        var list = new SimpleLockingList("TestList");

        list.IsExists("NONEXISTENT").Should().BeFalse();
    }

    [Fact]
    public void Add_SameCode_UpdatesExpiration()
    {
        var list = new SimpleLockingList("TestList");
        list.Add("CODE", TimeSpan.FromMinutes(1));
        var firstUnlockTime = list.GetUnlockTime("CODE");

        // Wait a tiny bit to ensure time difference
        Thread.Sleep(10);
        list.Add("CODE", TimeSpan.FromMinutes(10));
        var secondUnlockTime = list.GetUnlockTime("CODE");

        secondUnlockTime.Should().BeAfter(firstUnlockTime);
    }

    [Fact]
    public void Remove_ExistingCode_RemovesEntry()
    {
        var list = new SimpleLockingList("TestList");
        list.Add("CODE", TimeSpan.FromMinutes(5));

        list.Remove("CODE");

        list.IsExists("CODE").Should().BeFalse();
    }

    [Fact]
    public void Count_ReturnsCorrectCount()
    {
        var list = new SimpleLockingList("TestList");
        list.Add("CODE1", TimeSpan.FromMinutes(1));
        list.Add("CODE2", TimeSpan.FromMinutes(2));

        list.Count.Should().Be(2);
    }
}
