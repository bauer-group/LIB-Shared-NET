using BAUERGROUP.Shared.Core.DataManagement;

namespace BAUERGROUP.Shared.Test.Core;

public class FilteredStringCollectorTests
{
    [Fact]
    public void Add_ValidString_ReturnsTrue()
    {
        var collector = new FilteredStringCollector();
        var result = collector.Add("test");

        result.Should().BeTrue();
    }

    [Fact]
    public void Add_NullOrEmpty_ReturnsFalse()
    {
        var collector = new FilteredStringCollector();

        collector.Add(null!).Should().BeFalse();
        collector.Add("").Should().BeFalse();
        collector.Add("   ").Should().BeFalse();
    }

    [Fact]
    public void AllRecordsCount_ReturnsCorrectCount()
    {
        var collector = new FilteredStringCollector();
        collector.Add("one");
        collector.Add("two");
        collector.Add("three");

        collector.AllRecordsCount.Should().Be(3);
    }

    [Fact]
    public void AllRecords_ReturnsAllAddedRecords()
    {
        var collector = new FilteredStringCollector();
        collector.Add("one");
        collector.Add("two");

        collector.AllRecords.Should().Contain("one");
        collector.AllRecords.Should().Contain("two");
    }

    [Fact]
    public void FirstRecord_ReturnsFirstAdded()
    {
        var collector = new FilteredStringCollector();
        collector.Add("first");
        collector.Add("second");

        collector.FirstRecord.Should().Be("first");
    }

    [Fact]
    public void LastRecord_ReturnsLastAdded()
    {
        var collector = new FilteredStringCollector();
        collector.Add("first");
        collector.Add("last");

        collector.LastRecord.Should().Be("last");
    }

    [Fact]
    public void Contains_ExistingEntry_ReturnsTrue()
    {
        var collector = new FilteredStringCollector();
        collector.Add("test");

        collector.Contains("test").Should().BeTrue();
    }

    [Fact]
    public void Contains_NonExistingEntry_ReturnsFalse()
    {
        var collector = new FilteredStringCollector();
        collector.Add("test");

        collector.Contains("other").Should().BeFalse();
    }

    [Fact]
    public void Clear_RemovesAllRecords()
    {
        var collector = new FilteredStringCollector();
        collector.Add("one");
        collector.Add("two");
        collector.Clear();

        collector.AllRecordsCount.Should().Be(0);
    }

    [Fact]
    public void WithFilter_MatchingRecordsCount_FiltersCorrectly()
    {
        var collector = new FilteredStringCollector(@"^test");
        collector.Add("test1");
        collector.Add("test2");
        collector.Add("other");

        collector.MatchingRecordsCount.Should().Be(2);
        collector.AllRecordsCount.Should().Be(3);
    }

    [Fact]
    public void WithFilter_MatchingRecords_ReturnsOnlyMatching()
    {
        var collector = new FilteredStringCollector(@"error");
        collector.Add("error: something failed");
        collector.Add("info: everything ok");
        collector.Add("error: another issue");

        collector.MatchingRecords.Should().HaveCount(2);
        collector.MatchingRecords.Should().AllSatisfy(r => r.Should().Contain("error"));
    }

    [Fact]
    public void WithFilter_FirstMatchingRecord_ReturnsFirstMatch()
    {
        var collector = new FilteredStringCollector(@"^match");
        collector.Add("nomatch1");
        collector.Add("match1");
        collector.Add("match2");

        collector.FirstMatchingRecord.Should().Be("match1");
    }

    [Fact]
    public void WithFilter_LastMatchingRecord_ReturnsLastMatch()
    {
        var collector = new FilteredStringCollector(@"^match");
        collector.Add("match1");
        collector.Add("nomatch");
        collector.Add("match2");

        collector.LastMatchingRecord.Should().Be("match2");
    }

    [Fact]
    public void RecordAdded_Event_FiresOnAdd()
    {
        var collector = new FilteredStringCollector();
        FilteredStringCollectorEventArgs? receivedArgs = null;
        collector.RecordAdded += (s, e) => receivedArgs = e;

        collector.Add("test");

        receivedArgs.Should().NotBeNull();
        receivedArgs!.Entry.Should().Be("test");
    }

    [Fact]
    public void RecordAdded_Event_IndicatesMatch()
    {
        var collector = new FilteredStringCollector(@"^match");
        var events = new List<FilteredStringCollectorEventArgs>();
        collector.RecordAdded += (s, e) => events.Add(e);

        collector.Add("match");
        collector.Add("nomatch");

        events[0].Match.Should().BeTrue();
        events[1].Match.Should().BeFalse();
    }

    [Fact]
    public void WithoutFilter_AllRecordsAreMatching()
    {
        var collector = new FilteredStringCollector();
        collector.Add("one");
        collector.Add("two");
        collector.Add("three");

        collector.MatchingRecordsCount.Should().Be(collector.AllRecordsCount);
    }
}
