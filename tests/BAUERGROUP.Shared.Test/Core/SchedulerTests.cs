using BAUERGROUP.Shared.Core.PeriodicExecution;

namespace BAUERGROUP.Shared.Test.Core;

public class SchedulerTests : IDisposable
{
    private readonly Scheduler _scheduler;

    public SchedulerTests()
    {
        _scheduler = new Scheduler();
    }

    public void Dispose()
    {
        _scheduler.Dispose();
    }

    [Fact]
    public void Constructor_CreatesEmptyJobsList()
    {
        _scheduler.Jobs.Should().BeEmpty();
    }

    [Fact]
    public void RegisterJob_AddsJobToList()
    {
        var job = new TestSchedulerJob();

        _scheduler.RegisterJob(job);

        _scheduler.Jobs.Should().Contain(job);
    }

    [Fact]
    public void RegisterJob_WithNull_ThrowsArgumentNullException()
    {
        var action = () => _scheduler.RegisterJob(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RegisterJob_SameJobTwice_OnlyAddsOnce()
    {
        var job = new TestSchedulerJob();

        _scheduler.RegisterJob(job);
        _scheduler.RegisterJob(job);

        _scheduler.Jobs.Should().HaveCount(1);
    }

    [Fact]
    public void RegisterJob_MultipleJobs_AddsAll()
    {
        var job1 = new TestSchedulerJob();
        var job2 = new TestSchedulerJob();
        var job3 = new TestSchedulerJob();

        _scheduler.RegisterJob(job1);
        _scheduler.RegisterJob(job2);
        _scheduler.RegisterJob(job3);

        _scheduler.Jobs.Should().HaveCount(3);
    }

    [Fact]
    public void UnregisterJob_RemovesJobFromList()
    {
        var job = new TestSchedulerJob();
        _scheduler.RegisterJob(job);

        _scheduler.UnregisterJob(job);

        _scheduler.Jobs.Should().NotContain(job);
    }

    [Fact]
    public void UnregisterJob_WithNull_ThrowsArgumentNullException()
    {
        var action = () => _scheduler.UnregisterJob(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UnregisterJob_NonExistentJob_DoesNotThrow()
    {
        var job = new TestSchedulerJob();

        var action = () => _scheduler.UnregisterJob(job);

        action.Should().NotThrow();
    }

    [Fact]
    public async Task StartAsync_ExecutesEnabledJobs()
    {
        var job = new TestSchedulerJob { Interval = TimeSpan.FromMilliseconds(100) };
        _scheduler.RegisterJob(job);

        await _scheduler.StartAsync();
        await Task.Delay(500);
        await _scheduler.StopAsync();

        job.ExecutionCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task StartAsync_DoesNotExecuteDisabledJobs()
    {
        var job = new TestSchedulerJob { Enabled = false, Interval = TimeSpan.FromMilliseconds(100) };
        _scheduler.RegisterJob(job);

        await _scheduler.StartAsync();
        await Task.Delay(500);
        await _scheduler.StopAsync();

        job.ExecutionCount.Should().Be(0);
    }

    [Fact]
    public async Task StopAsync_StopsJobExecution()
    {
        var job = new TestSchedulerJob { Interval = TimeSpan.FromMilliseconds(100) };
        _scheduler.RegisterJob(job);

        await _scheduler.StartAsync();
        await Task.Delay(500);
        await _scheduler.StopAsync();
        var countAfterStop = job.ExecutionCount;
        await Task.Delay(300);

        job.ExecutionCount.Should().Be(countAfterStop);
    }

    [Fact]
    public async Task RestartAsync_StopsAndStartsAgain()
    {
        var job = new TestSchedulerJob { Interval = TimeSpan.FromMilliseconds(100) };
        _scheduler.RegisterJob(job);

        await _scheduler.StartAsync();
        await WaitForExecutionsAsync(job, atLeast: 1, timeout: TimeSpan.FromSeconds(5));
        var countBeforeRestart = job.ExecutionCount;

        await _scheduler.RestartAsync();
        await WaitForExecutionsAsync(job, atLeast: countBeforeRestart + 1, timeout: TimeSpan.FromSeconds(5));
        await _scheduler.StopAsync();

        job.ExecutionCount.Should().BeGreaterThan(1);
    }

    private static async Task WaitForExecutionsAsync(TestSchedulerJob job, int atLeast, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (job.ExecutionCount < atLeast)
        {
            if (DateTime.UtcNow > deadline)
                throw new TimeoutException($"Job did not reach ExecutionCount >= {atLeast} within {timeout.TotalMilliseconds}ms (actual: {job.ExecutionCount})");
            await Task.Delay(20);
        }
    }

    [Fact]
    public async Task Start_SetsLastExecution()
    {
        var job = new TestSchedulerJob { Interval = TimeSpan.FromMilliseconds(100) };
        _scheduler.RegisterJob(job);
        job.LastExecution.Should().BeNull();

        await _scheduler.StartAsync();
        await Task.Delay(500);
        await _scheduler.StopAsync();

        job.LastExecution.Should().NotBeNull();
    }

    [Fact]
    public async Task Start_SetsSuccessfulExecutionOnSuccess()
    {
        var job = new TestSchedulerJob { Interval = TimeSpan.FromMilliseconds(100) };
        _scheduler.RegisterJob(job);

        await _scheduler.StartAsync();
        await Task.Delay(500);
        await _scheduler.StopAsync();

        job.SuccessfulExecution.Should().BeTrue();
    }

    [Fact]
    public async Task Start_SetsSuccessfulExecutionFalseOnFailure()
    {
        var job = new FailingSchedulerJob { Interval = TimeSpan.FromMilliseconds(100) };
        _scheduler.RegisterJob(job);

        await _scheduler.StartAsync();
        await Task.Delay(500);
        await _scheduler.StopAsync();

        job.SuccessfulExecution.Should().BeFalse();
    }

    [Fact]
    public void Dispose_StopsScheduler()
    {
        var scheduler = new Scheduler();
        var job = new TestSchedulerJob { Interval = TimeSpan.FromMilliseconds(50) };
        scheduler.RegisterJob(job);
        scheduler.Start();

        scheduler.Dispose();

        // Disposed scheduler should not throw on access
        scheduler.Jobs.Should().NotBeNull();
    }

    [Fact]
    public void Jobs_ReturnsReadOnlyList()
    {
        var job = new TestSchedulerJob();
        _scheduler.RegisterJob(job);

        var jobs = _scheduler.Jobs;

        jobs.Should().BeAssignableTo<IReadOnlyList<ISchedulerObject>>();
    }

    private class TestSchedulerJob : SchedulerObject
    {
        public int ExecutionCount { get; private set; }

        public override void Execute()
        {
            ExecutionCount++;
        }
    }

    private class FailingSchedulerJob : SchedulerObject
    {
        public override void Execute()
        {
            throw new InvalidOperationException("Intentional test failure");
        }
    }
}
