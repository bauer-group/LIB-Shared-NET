using BAUERGROUP.Shared.Core.PeriodicExecution;

namespace BAUERGROUP.Shared.Test.Core;

public class SchedulerObjectTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var obj = new SchedulerObject();

        obj.Uid.Should().NotBe(Guid.Empty);
        obj.Interval.Should().Be(TimeSpan.FromMinutes(1));
        obj.Enabled.Should().BeTrue();
        obj.LastExecution.Should().BeNull();
        obj.SuccessfulExecution.Should().BeFalse();
        obj.LastChange.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        obj.IsCanceled.Should().BeFalse();
        obj.Parameters.Should().BeNull();
    }

    [Fact]
    public void Uid_CanBeSet()
    {
        var obj = new SchedulerObject();
        var newGuid = Guid.NewGuid();

        obj.Uid = newGuid;

        obj.Uid.Should().Be(newGuid);
    }

    [Fact]
    public void Interval_CanBeSet()
    {
        var obj = new SchedulerObject();

        obj.Interval = TimeSpan.FromHours(2);

        obj.Interval.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void Enabled_CanBeSet()
    {
        var obj = new SchedulerObject();

        obj.Enabled = false;

        obj.Enabled.Should().BeFalse();
    }

    [Fact]
    public void LastExecution_CanBeSet()
    {
        var obj = new SchedulerObject();
        var executionTime = DateTime.UtcNow;

        obj.LastExecution = executionTime;

        obj.LastExecution.Should().Be(executionTime);
    }

    [Fact]
    public void SuccessfulExecution_CanBeSet()
    {
        var obj = new SchedulerObject();

        obj.SuccessfulExecution = true;

        obj.SuccessfulExecution.Should().BeTrue();
    }

    [Fact]
    public void LastChange_CanBeSet()
    {
        var obj = new SchedulerObject();
        var changeTime = DateTime.UtcNow.AddDays(-1);

        obj.LastChange = changeTime;

        obj.LastChange.Should().Be(changeTime);
    }

    [Fact]
    public void IsCanceled_CanBeSet()
    {
        var obj = new SchedulerObject();

        obj.IsCanceled = true;

        obj.IsCanceled.Should().BeTrue();
    }

    [Fact]
    public void IsCanceled_IsThreadSafe()
    {
        var obj = new SchedulerObject();
        var iterations = 1000;
        var setTasks = new Task[iterations];
        var getTasks = new Task<bool>[iterations];

        for (int i = 0; i < iterations; i++)
        {
            var value = i % 2 == 0;
            setTasks[i] = Task.Run(() => obj.IsCanceled = value);
            getTasks[i] = Task.Run(() => obj.IsCanceled);
        }

        var action = () =>
        {
            Task.WaitAll(setTasks);
            Task.WaitAll(getTasks);
        };

        action.Should().NotThrow();
    }

    [Fact]
    public void Parameters_CanBeSet()
    {
        var obj = new SchedulerObject();
        var parameters = new { Key = "Value", Count = 42 };

        obj.Parameters = parameters;

        obj.Parameters.Should().Be(parameters);
    }

    [Fact]
    public void Execute_DoesNotThrow()
    {
        var obj = new SchedulerObject();

        var action = () => obj.Execute();

        action.Should().NotThrow();
    }

    [Fact]
    public void Start_DoesNotThrow()
    {
        var obj = new SchedulerObject();

        var action = () => obj.Start();

        action.Should().NotThrow();
    }

    [Fact]
    public void Stop_DoesNotThrow()
    {
        var obj = new SchedulerObject();

        var action = () => obj.Stop();

        action.Should().NotThrow();
    }

    [Fact]
    public void DerivedClass_CanOverrideExecute()
    {
        var obj = new CustomSchedulerObject();

        obj.Execute();

        obj.CustomExecutionPerformed.Should().BeTrue();
    }

    private class TestableSchedulerObject : SchedulerObject
    {
        public bool WasExecuted { get; private set; }

        public override void Execute()
        {
            WasExecuted = true;
        }
    }

    private class CustomSchedulerObject : SchedulerObject
    {
        public bool CustomExecutionPerformed { get; private set; }

        public override void Execute()
        {
            CustomExecutionPerformed = true;
        }
    }
}
