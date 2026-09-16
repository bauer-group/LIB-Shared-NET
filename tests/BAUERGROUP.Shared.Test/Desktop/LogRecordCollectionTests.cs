using System.Collections.Specialized;
using BAUERGROUP.Shared.Core.Logging;
using BAUERGROUP.Shared.Desktop.Logging;
using NLog;

namespace BAUERGROUP.Shared.Test.Desktop;

// LogRecordCollection is the batch-update model behind the WPF log viewer: it turns one BGLogViewUpdate
// into as few CollectionChanged notifications as WPF accepts (per item while tailing, Reset for large batches).
public class LogRecordCollectionTests
{
    private static BGLogRecord Record(string message) =>
        new BGLogRecord(new DateTime(2026, 9, 16, 10, 0, 0), LogLevel.Info, "Test", message);

    private static BGLogViewUpdate Drain(BGLogViewBuffer buffer, params string[] messages)
    {
        foreach (var message in messages)
            buffer.Post(Record(message));

        return buffer.Drain();
    }

    private static BGLogViewBuffer NewBuffer(int maxLines) =>
        new BGLogViewBuffer(maxLines, LogLevel.Off, (_, _) => new NoopRegistration());

    private sealed class NoopRegistration : IDisposable
    {
        public void Dispose() { }
    }

    private static List<NotifyCollectionChangedAction> RecordEvents(LogRecordCollection collection)
    {
        var actions = new List<NotifyCollectionChangedAction>();
        ((INotifyCollectionChanged)collection).CollectionChanged += (_, e) => actions.Add(e.Action);
        return actions;
    }

    [Fact]
    public void Apply_EmptyUpdate_ShouldNotRaiseEvents()
    {
        var collection = new LogRecordCollection();
        var actions = RecordEvents(collection);

        collection.Apply(default);

        collection.Should().BeEmpty();
        actions.Should().BeEmpty();
    }

    [Fact]
    public void Apply_SmallBatch_ShouldAppendPerItem()
    {
        using var buffer = NewBuffer(100);
        var collection = new LogRecordCollection();
        var actions = RecordEvents(collection);

        collection.Apply(Drain(buffer, "a", "b", "c"));

        collection.Select(r => r.Message).Should().Equal("a", "b", "c");
        actions.Should().AllSatisfy(a => a.Should().Be(NotifyCollectionChangedAction.Add));
    }

    [Fact]
    public void Apply_LargeBatch_ShouldUseASingleReset()
    {
        using var buffer = NewBuffer(1000);
        var collection = new LogRecordCollection();
        var actions = RecordEvents(collection);

        collection.Apply(Drain(buffer, Enumerable.Range(0, 200).Select(i => i.ToString()).ToArray()));

        collection.Should().HaveCount(200);
        actions.Should().ContainSingle().Which.Should().Be(NotifyCollectionChangedAction.Reset);
    }

    [Fact]
    public void Apply_Overflow_ShouldDropOldestAndKeepNewest()
    {
        using var buffer = NewBuffer(10);
        var collection = new LogRecordCollection();
        collection.Apply(Drain(buffer, Enumerable.Range(0, 10).Select(i => i.ToString()).ToArray()));

        collection.Apply(Drain(buffer, "10", "11", "12"));

        collection.Should().HaveCount(10);
        collection.Select(r => r.Message).Should().Equal(Enumerable.Range(3, 10).Select(i => i.ToString()));
    }

    [Fact]
    public void Apply_RemoveFromStartBeyondCount_ShouldNotThrow()
    {
        var collection = new LogRecordCollection { Record("only") };
        using var buffer = NewBuffer(5);
        buffer.Post(Record("next"));
        buffer.MaxLines = 1;
        var update = buffer.Drain();

        var apply = () => collection.Apply(update);

        // The view count inside the buffer does not know what the collection holds, so the clamp matters
        apply.Should().NotThrow();
    }

    [Fact]
    public void Apply_Reset_ShouldReplaceTheWholeView()
    {
        using var buffer = NewBuffer(100);
        var collection = new LogRecordCollection();
        collection.Apply(Drain(buffer, "old"));
        buffer.Clear();
        var actions = RecordEvents(collection);

        collection.Apply(Drain(buffer, "fresh"));

        collection.Select(r => r.Message).Should().Equal("fresh");
        actions.Should().ContainSingle().Which.Should().Be(NotifyCollectionChangedAction.Reset);
    }
}
