using System.Runtime.CompilerServices;
using BAUERGROUP.Shared.Core.Logging;
using NLog;

namespace BAUERGROUP.Shared.Test.Core;

public class BGLogRingBufferTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Ctor_CapacityBelowOne_ShouldThrow(int capacity)
    {
        Action act = () => new BGLogRingBuffer(capacity);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Add_UnderCapacity_ShouldPreserveOrder()
    {
        var buffer = new BGLogRingBuffer(8);
        AddRange(buffer, 1, 2, 3);

        buffer.Count.Should().Be(3);
        buffer.DroppedCount.Should().Be(0);

        var drained = Drain(buffer);

        Messages(drained).Should().Equal("1", "2", "3");
        buffer.Count.Should().Be(0);
    }

    [Fact]
    public void Add_OverCapacity_ShouldDropOldestAndCountDropped()
    {
        var buffer = new BGLogRingBuffer(3);
        AddRange(buffer, 1, 2, 3, 4, 5);

        buffer.Count.Should().Be(3);
        buffer.DroppedCount.Should().Be(2);
        Messages(Drain(buffer)).Should().Equal("3", "4", "5");
    }

    [Fact]
    public void Add_AfterDrain_ShouldWrapCorrectly()
    {
        var buffer = new BGLogRingBuffer(4);
        AddRange(buffer, 1, 2, 3);
        Messages(Drain(buffer)).Should().Equal("1", "2", "3");

        AddRange(buffer, 4, 5, 6, 7, 8);

        buffer.DroppedCount.Should().Be(1);
        Messages(Drain(buffer)).Should().Equal("5", "6", "7", "8");
    }

    [Fact]
    public void Resize_Smaller_ShouldKeepNewest_AndCountTheDiscardedRecords()
    {
        var buffer = new BGLogRingBuffer(8);
        AddRange(buffer, 1, 2, 3, 4, 5);

        buffer.Resize(2);

        buffer.Capacity.Should().Be(2);
        buffer.Count.Should().Be(2);

        // Records that no longer fit the smaller bound are lost, so they count as dropped just like an
        // overflow - the viewer's dropped counter must not understate what never reached the view.
        buffer.DroppedCount.Should().Be(3);
        Messages(Drain(buffer)).Should().Equal("4", "5");
    }

    [Fact]
    public void Resize_Larger_ShouldKeepAll()
    {
        var buffer = new BGLogRingBuffer(3);
        AddRange(buffer, 1, 2, 3);

        buffer.Resize(10);

        buffer.Capacity.Should().Be(10);
        buffer.Count.Should().Be(3);

        AddRange(buffer, 4);
        Messages(Drain(buffer)).Should().Equal("1", "2", "3", "4");
    }

    [Fact]
    public void Clear_ShouldEmptyBufferAndKeepDroppedCount()
    {
        var buffer = new BGLogRingBuffer(2);
        AddRange(buffer, 1, 2, 3);

        buffer.Clear();

        buffer.Count.Should().Be(0);
        buffer.DroppedCount.Should().Be(1);
        Drain(buffer).Should().BeEmpty();
    }

    [Fact]
    public void DrainTo_ShouldReleaseDrainedSlots()
    {
        var buffer = new BGLogRingBuffer(4);
        var weak = AddCollectableRecord(buffer);

        DrainAndDiscard(buffer);

        for (var attempt = 0; attempt < 5 && weak.IsAlive; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        weak.IsAlive.Should().BeFalse();
    }

    [Fact]
    public void ConcurrentAdd_FourThreadsTenThousandEach_ShouldNeverExceedCapacity_AndAccountEveryRecord()
    {
        const int capacity = 256;
        const int threadCount = 4;
        const int perThread = 10_000;

        var buffer = new BGLogRingBuffer(capacity);
        var drained = new List<BGLogRecord>();
        var maximumObservedCount = 0;

        using var producersFinished = new ManualResetEventSlim(false);

        // A single consumer thread owns "drained" and "maximumObservedCount".
        var consumer = new Thread(() =>
        {
            while (!producersFinished.IsSet)
            {
                maximumObservedCount = Math.Max(maximumObservedCount, buffer.Count);
                buffer.DrainTo(drained);
            }

            buffer.DrainTo(drained);
        })
        {
            IsBackground = true
        };

        consumer.Start();

        var producers = Enumerable.Range(0, threadCount)
            .Select(thread => new Thread(() =>
            {
                for (var i = 0; i < perThread; i++)
                    buffer.Add(Record($"{thread}-{i}"));
            })
            {
                IsBackground = true
            })
            .ToArray();

        foreach (var producer in producers)
            producer.Start();

        foreach (var producer in producers)
            producer.Join();

        producersFinished.Set();
        consumer.Join();

        maximumObservedCount.Should().BeLessThanOrEqualTo(capacity);
        buffer.Count.Should().Be(0);
        (drained.Count + buffer.DroppedCount).Should().Be(threadCount * perThread);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AddCollectableRecord(BGLogRingBuffer buffer)
    {
        var record = Record("collectable");
        buffer.Add(record);
        return new WeakReference(record);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DrainAndDiscard(BGLogRingBuffer buffer)
    {
        var destination = new List<BGLogRecord>();
        buffer.DrainTo(destination);
        destination.Clear();
    }

    private static void AddRange(BGLogRingBuffer buffer, params int[] numbers)
    {
        foreach (var number in numbers)
            buffer.Add(Record(number.ToString()));
    }

    private static List<BGLogRecord> Drain(BGLogRingBuffer buffer)
    {
        var destination = new List<BGLogRecord>();
        buffer.DrainTo(destination);
        return destination;
    }

    private static IEnumerable<string> Messages(IEnumerable<BGLogRecord> records) => records.Select(record => record.Message);

    private static BGLogRecord Record(string message) => new(DateTime.Now, LogLevel.Info, "Test", message);
}
