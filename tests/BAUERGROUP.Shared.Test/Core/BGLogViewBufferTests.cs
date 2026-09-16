using BAUERGROUP.Shared.Core.Logging;
using NLog;

namespace BAUERGROUP.Shared.Test.Core;

/// <summary>
/// Exercises <see cref="BGLogViewBuffer"/> through an injected registration factory: no NLog, no
/// process-global state, fully deterministic.
/// </summary>
public class BGLogViewBufferTests
{
    [Fact]
    public void Attach_Twice_ShouldSubscribeOnce()
    {
        var subscriber = new RecordingSubscriber();
        using var buffer = new BGLogViewBuffer(10, null, subscriber.Subscribe);

        buffer.Attach();
        buffer.Attach();

        subscriber.Registrations.Should().ContainSingle();
        buffer.IsAttached.Should().BeTrue();
    }

    [Fact]
    public void Detach_Twice_ShouldDisposeOnce()
    {
        var subscriber = new RecordingSubscriber();
        var buffer = new BGLogViewBuffer(10, null, subscriber.Subscribe);
        buffer.Attach();

        buffer.Detach();
        buffer.Detach();
        buffer.Dispose();

        subscriber.Registrations.Should().ContainSingle();
        subscriber.Registrations[0].DisposeCount.Should().Be(1);
        buffer.IsAttached.Should().BeFalse();
    }

    [Fact]
    public void StaleSink_AfterDetach_ShouldNotEnqueue()
    {
        var subscriber = new RecordingSubscriber();
        using var buffer = new BGLogViewBuffer(10, null, subscriber.Subscribe);
        buffer.Attach();
        var staleSink = subscriber.Registrations[0].Sink;

        buffer.Detach();
        staleSink(Record("after detach"));
        buffer.PendingCount.Should().Be(0);

        buffer.Attach();
        staleSink(Record("after re-attach"));
        buffer.PendingCount.Should().Be(0);

        subscriber.Registrations[1].Sink(Record("current"));
        buffer.PendingCount.Should().Be(1);
    }

    [Fact]
    public void Drain_ShouldReturnRecordsInPostOrder_AndEmptyPending()
    {
        using var buffer = new BGLogViewBuffer(10, null, null);
        Post(buffer, 1, 2, 3);

        var update = buffer.Drain();

        update.IsReset.Should().BeFalse();
        update.RemoveFromStart.Should().Be(0);
        Messages(update).Should().Equal("1", "2", "3");
        buffer.PendingCount.Should().Be(0);
    }

    [Fact]
    public void Drain_WhenIdle_ShouldReturnEmptyUpdate()
    {
        using var buffer = new BGLogViewBuffer(10, null, null);

        var update = buffer.Drain();

        update.IsEmpty.Should().BeTrue();
        update.IsReset.Should().BeFalse();
        update.RemoveFromStart.Should().Be(0);
        update.Added.Should().NotBeNull();
        update.Added.Should().BeEmpty();
    }

    [Fact]
    public void DefaultUpdate_ShouldBeEmpty()
    {
        var update = default(BGLogViewUpdate);

        update.IsEmpty.Should().BeTrue();
        update.Added.Should().NotBeNull();
        update.Added.Should().BeEmpty();
    }

    [Fact]
    public void Drain_BeyondMaxLines_ShouldReportRemoveFromStart()
    {
        using var buffer = new BGLogViewBuffer(5, null, null);
        Post(buffer, 1, 2, 3);
        buffer.Drain();

        Post(buffer, 4, 5, 6, 7);
        var update = buffer.Drain();

        update.IsReset.Should().BeFalse();
        update.RemoveFromStart.Should().Be(2);
        Messages(update).Should().Equal("4", "5", "6", "7");
    }

    [Fact]
    public void Burst_LargerThanMaxLines_ShouldKeepNewestAndCountDropped()
    {
        using var buffer = new BGLogViewBuffer(4, null, null);
        Post(buffer, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);

        buffer.DroppedCount.Should().Be(6);

        var update = buffer.Drain();

        Messages(update).Should().Equal("7", "8", "9", "10");
    }

    [Fact]
    public void MaxLines_Decrease_ShouldTrimViewOnNextDrain_WithoutPendingRecords()
    {
        using var buffer = new BGLogViewBuffer(10, null, null);
        Post(buffer, 1, 2, 3, 4, 5, 6);
        buffer.Drain();

        buffer.MaxLines = 4;
        buffer.PendingCount.Should().Be(0);

        var update = buffer.Drain();

        update.IsReset.Should().BeFalse();
        update.RemoveFromStart.Should().Be(2);
        update.Added.Should().BeEmpty();
        update.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void MaxLines_Decrease_WithPendingRecords_ShouldDiscardTheOldestAndCountThem()
    {
        using var buffer = new BGLogViewBuffer(10, null, null);
        Post(buffer, 1, 2, 3, 4, 5, 6);

        buffer.DroppedCount.Should().Be(0);

        buffer.MaxLines = 2;

        buffer.PendingCount.Should().Be(2);
        buffer.DroppedCount.Should().Be(4);
        Messages(buffer.Drain()).Should().Equal("5", "6");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MaxLines_BelowOne_ShouldThrow(int maxLines)
    {
        using var buffer = new BGLogViewBuffer(10, null, null);

        Action act = () => buffer.MaxLines = maxLines;

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Ctor_MaxLinesBelowOne_ShouldThrow()
    {
        Action act = () => new BGLogViewBuffer(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Post_Null_ShouldThrow()
    {
        using var buffer = new BGLogViewBuffer(10, null, null);

        Action act = () => buffer.Post(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MinimumLevel_Null_ShouldThrow()
    {
        using var buffer = new BGLogViewBuffer(10, null, null);

        Action act = () => buffer.MinimumLevel = null!;

        act.Should().Throw<ArgumentNullException>();
        buffer.MinimumLevel.Should().Be(LogLevel.Trace);
    }

    [Fact]
    public void Clear_ShouldEmitReset_AndResetViewCount()
    {
        using var buffer = new BGLogViewBuffer(10, null, null);
        Post(buffer, 1, 2, 3);
        buffer.Drain();

        Post(buffer, 4);
        buffer.Clear();
        buffer.PendingCount.Should().Be(0);

        Post(buffer, 5, 6);
        var update = buffer.Drain();

        update.IsReset.Should().BeTrue();
        update.RemoveFromStart.Should().Be(0);
        Messages(update).Should().Equal("5", "6");

        Post(buffer, 7);
        var next = buffer.Drain();

        next.IsReset.Should().BeFalse();
        next.RemoveFromStart.Should().Be(0);
        Messages(next).Should().Equal("7");
    }

    [Fact]
    public void MinimumLevel_SetWhileAttached_ShouldReRegisterWithNewLevel()
    {
        var subscriber = new RecordingSubscriber();
        using var buffer = new BGLogViewBuffer(10, null, subscriber.Subscribe);
        buffer.Attach();

        buffer.MinimumLevel = LogLevel.Warn;

        subscriber.Registrations.Should().HaveCount(2);
        subscriber.Registrations[0].MinimumLevel.Should().Be(LogLevel.Trace);
        subscriber.Registrations[0].DisposeCount.Should().Be(1);
        subscriber.Registrations[1].MinimumLevel.Should().Be(LogLevel.Warn);
        subscriber.Registrations[1].DisposeCount.Should().Be(0);
        buffer.MinimumLevel.Should().Be(LogLevel.Warn);
    }

    [Fact]
    public void MinimumLevel_SetWhileDetached_ShouldNotRegister()
    {
        var subscriber = new RecordingSubscriber();
        using var buffer = new BGLogViewBuffer(10, null, subscriber.Subscribe);

        buffer.MinimumLevel = LogLevel.Error;

        subscriber.Registrations.Should().BeEmpty();

        buffer.Attach();

        subscriber.Registrations.Should().ContainSingle();
        subscriber.Registrations[0].MinimumLevel.Should().Be(LogLevel.Error);
    }

    [Fact]
    public void RandomOperations_AppliedUpdates_ShouldAlwaysMirrorExpectedView()
    {
        const int initialMaxLines = 16;

        var random = new Random(20260916);
        using var buffer = new BGLogViewBuffer(initialMaxLines, null, null);

        // The view is built ONLY by applying the returned updates and is compared against a trivially
        // correct list model of the same buffer.
        var view = new List<BGLogRecord>();
        var model = new ViewModel(initialMaxLines);
        var sequence = 0;

        for (var step = 0; step < 10_000; step++)
        {
            switch (random.Next(4))
            {
                case 0:
                    var count = random.Next(1, 6);
                    for (var i = 0; i < count; i++)
                    {
                        var record = Record((++sequence).ToString());
                        buffer.Post(record);
                        model.Post(record);
                    }

                    break;

                case 1:
                    DrainAndVerify(buffer, view, model);
                    break;

                case 2:
                    var maxLines = random.Next(1, 33);
                    buffer.MaxLines = maxLines;
                    model.SetMaxLines(maxLines);
                    break;

                default:
                    buffer.Clear();
                    model.Clear();
                    break;
            }
        }

        DrainAndVerify(buffer, view, model);
    }

    private static void DrainAndVerify(BGLogViewBuffer buffer, List<BGLogRecord> view, ViewModel model)
    {
        Apply(view, buffer.Drain());
        model.Drain();

        view.Should().Equal(model.View);
        buffer.PendingCount.Should().Be(0);

        // While nothing was dropped, the view must be exactly the newest min(total, MaxLines) posted records.
        if (model.IsComplete)
        {
            var start = Math.Max(0, model.Posted.Count - buffer.MaxLines);
            view.Should().Equal(model.Posted.GetRange(start, model.Posted.Count - start));
        }
    }

    private static void Apply(List<BGLogRecord> view, BGLogViewUpdate update)
    {
        if (update.IsReset)
            view.Clear();
        else if (update.RemoveFromStart > 0)
            view.RemoveRange(0, update.RemoveFromStart);

        if (update.Added.Count > 0)
            view.AddRange(update.Added);
    }

    private static void Post(BGLogViewBuffer buffer, params int[] numbers)
    {
        foreach (var number in numbers)
            buffer.Post(Record(number.ToString()));
    }

    private static IEnumerable<string> Messages(BGLogViewUpdate update) => update.Added.Select(record => record.Message);

    private static BGLogRecord Record(string message) => new(DateTime.Now, LogLevel.Info, "Test", message);

    /// <summary>
    /// Straightforward list model of a <see cref="BGLogViewBuffer"/>: pending records bounded by MaxLines
    /// (drop oldest), the view fed by drains and trimmed to MaxLines.
    /// </summary>
    private sealed class ViewModel
    {
        private readonly List<BGLogRecord> _pending = new();
        private int _maxLines;
        private int _smallestBoundAtDiscard = int.MaxValue;

        public ViewModel(int maxLines) => _maxLines = maxLines;

        public List<BGLogRecord> View { get; } = new();

        public List<BGLogRecord> Posted { get; } = new();

        /// <summary>
        /// True while every discard so far happened at a bound of at least the current MaxLines — only then
        /// must the view still hold the newest min(total, MaxLines) posted records. Raising MaxLines after a
        /// discard does not bring the discarded records back.
        /// </summary>
        public bool IsComplete => _maxLines <= _smallestBoundAtDiscard;

        public void Post(BGLogRecord record)
        {
            Posted.Add(record);
            _pending.Add(record);
            TrimPending();
        }

        public void SetMaxLines(int maxLines)
        {
            _maxLines = maxLines;
            TrimPending();
        }

        public void Drain()
        {
            View.AddRange(_pending);
            _pending.Clear();

            if (View.Count > _maxLines)
            {
                View.RemoveRange(0, View.Count - _maxLines);
                NoteDiscard();
            }
        }

        public void Clear()
        {
            _pending.Clear();
            View.Clear();
            Posted.Clear();
            _smallestBoundAtDiscard = int.MaxValue;
        }

        private void TrimPending()
        {
            if (_pending.Count <= _maxLines)
                return;

            _pending.RemoveRange(0, _pending.Count - _maxLines);
            NoteDiscard();
        }

        private void NoteDiscard() => _smallestBoundAtDiscard = Math.Min(_smallestBoundAtDiscard, _maxLines);
    }

    private sealed class RecordingSubscriber
    {
        public List<Registration> Registrations { get; } = new();

        public IDisposable Subscribe(Action<BGLogRecord> sink, LogLevel minimumLevel)
        {
            var registration = new Registration(sink, minimumLevel);
            Registrations.Add(registration);
            return registration;
        }

        public sealed class Registration : IDisposable
        {
            public Registration(Action<BGLogRecord> sink, LogLevel minimumLevel)
            {
                Sink = sink;
                MinimumLevel = minimumLevel;
            }

            public Action<BGLogRecord> Sink { get; }

            public LogLevel MinimumLevel { get; }

            public int DisposeCount { get; private set; }

            public void Dispose() => DisposeCount++;
        }
    }
}
