using System.Collections.Concurrent;
using BAUERGROUP.Shared.Core.Logging;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace BAUERGROUP.Shared.Test.Core;

/// <summary>
/// End-to-end tests for the live sink against the real NLog pipeline. Every assertion filters by a
/// per-test marker because other classes may log into the same configuration.
/// </summary>
[Collection("BGLogger")]
public class BGLoggerLiveSinkTests
{
    private const string LiveTargetName = "LIVE";

    [Fact]
    public void AddLiveSink_Null_ShouldThrow()
    {
        Action act = () => BGLogger.Configuration.AddLiveSink(null!);

        act.Should().Throw<ArgumentNullException>();
        BGLogger.Configuration.LiveSinkCount.Should().Be(0);
    }

    [Fact]
    public void AddLiveSink_ShouldDeliverRecordWithLevelAndMessage()
    {
        var collector = new Collector();

        using (BGLogger.Configuration.AddLiveSink(collector.Post))
        {
            BGLogger.Info($"info {collector.Marker}");
        }

        collector.Records.Should().ContainSingle();
        var record = collector.Records[0];
        record.Level.Should().Be(LogLevel.Info);
        record.Message.Should().Be($"info {collector.Marker}");
        record.Exception.Should().BeNull();
        record.IsWarning.Should().BeFalse();
        record.IsError.Should().BeFalse();
        record.LoggerName.Should().NotBeEmpty();
    }

    [Fact]
    public void AddLiveSink_WithException_ShouldCaptureExceptionText()
    {
        var collector = new Collector();

        using (BGLogger.Configuration.AddLiveSink(collector.Post))
        {
            BGLogger.Error(new InvalidOperationException("live sink probe"), $"failed {collector.Marker}");
        }

        collector.Records.Should().ContainSingle();
        var record = collector.Records[0];
        record.IsError.Should().BeTrue();
        record.Exception.Should().NotBeNull();
        record.Exception.Should().Contain(nameof(InvalidOperationException)).And.Contain("live sink probe");
        record.DisplayText.Should().Contain(record.Exception!);
    }

    [Fact]
    public void AddLiveSink_ShouldRegisterTargetNamedLive()
    {
        using (BGLogger.Configuration.AddLiveSink(_ => { }))
        {
            BGLogger.Configuration.LiveSinkCount.Should().Be(1);
            LiveTarget().Should().NotBeNull();
            LiveRule().Should().NotBeNull();
        }
    }

    [Fact]
    public void Dispose_LastSink_ShouldRemoveTargetAndRule()
    {
        var collector = new Collector();
        var registration = BGLogger.Configuration.AddLiveSink(collector.Post);

        registration.Dispose();

        BGLogger.Configuration.LiveSinkCount.Should().Be(0);
        LiveTarget().Should().BeNull();
        LiveRule().Should().BeNull();

        BGLogger.Info($"after dispose {collector.Marker}");

        collector.Records.Should().BeEmpty();
    }

    [Fact]
    public void Dispose_Twice_ShouldBeIdempotent()
    {
        var registration = BGLogger.Configuration.AddLiveSink(_ => { });

        registration.Dispose();
        registration.Dispose();

        BGLogger.Configuration.LiveSinkCount.Should().Be(0);
        LiveTarget().Should().BeNull();
    }

    [Fact]
    public void TwoSinks_DisposeOne_ShouldKeepTargetAndDeliverToTheOther()
    {
        var marker = NewMarker();
        var first = new Collector(marker);
        var second = new Collector(marker);

        var firstRegistration = BGLogger.Configuration.AddLiveSink(first.Post);
        try
        {
            using (BGLogger.Configuration.AddLiveSink(second.Post))
            {
                BGLogger.Configuration.LiveSinkCount.Should().Be(2);

                firstRegistration.Dispose();

                BGLogger.Configuration.LiveSinkCount.Should().Be(1);
                LiveTarget().Should().NotBeNull();

                BGLogger.Info($"only the second {marker}");
            }
        }
        finally
        {
            // A failed assertion above must not leak the registration into the next test in this collection.
            firstRegistration.Dispose();
        }

        first.Records.Should().BeEmpty();
        second.Records.Should().ContainSingle();
    }

    [Fact]
    public void SameDelegateRegisteredTwice_ShouldCreateTwoIndependentRegistrations()
    {
        var collector = new Collector();
        Action<BGLogRecord> sink = collector.Post;

        var firstRegistration = BGLogger.Configuration.AddLiveSink(sink);
        try
        {
            using (BGLogger.Configuration.AddLiveSink(sink))
            {
                BGLogger.Configuration.LiveSinkCount.Should().Be(2);

                BGLogger.Info($"twice {collector.Marker}");
                collector.Records.Should().HaveCount(2);

                firstRegistration.Dispose();
                BGLogger.Configuration.LiveSinkCount.Should().Be(1);

                BGLogger.Info($"once {collector.Marker}");
                collector.Records.Should().HaveCount(3);
            }
        }
        finally
        {
            firstRegistration.Dispose();
        }
    }

    [Fact]
    public void ReAddAfterFullRemoval_ShouldDeliverAgain()
    {
        var collector = new Collector();

        BGLogger.Configuration.AddLiveSink(collector.Post).Dispose();
        LiveTarget().Should().BeNull();

        using (BGLogger.Configuration.AddLiveSink(collector.Post))
        {
            LiveTarget().Should().NotBeNull();
            BGLogger.Info($"second registration {collector.Marker}");
        }

        collector.Records.Should().ContainSingle();
    }

    [Fact]
    public void ThrowingSink_ShouldNotAffectCallerOrOtherSinks()
    {
        var marker = NewMarker();
        var healthy = new Collector(marker);
        var throwCount = 0;

        using (BGLogger.Configuration.AddLiveSink(_ =>
               {
                   throwCount++;
                   throw new InvalidOperationException("sink failure");
               }))
        using (BGLogger.Configuration.AddLiveSink(healthy.Post))
        {
            Action act = () => BGLogger.Info($"throwing sink {marker}");

            act.Should().NotThrow();
        }

        throwCount.Should().BeGreaterThan(0);
        healthy.Records.Should().ContainSingle();
    }

    [Fact]
    public void SinkThatLogs_ShouldBeInvokedExactlyOnce_AndNotRecurse()
    {
        var marker = NewMarker();
        var collector = new Collector(marker);

        using (BGLogger.Configuration.AddLiveSink(record =>
               {
                   collector.Post(record);

                   // A re-entrancy guard failure would recurse forever - the bound turns it into a
                   // failed assertion instead of a stack overflow.
                   if (collector.Count <= 3)
                       BGLogger.Warn($"from inside the sink {marker}");
               }))
        {
            BGLogger.Info($"outer {marker}");
        }

        collector.Records.Should().ContainSingle();
        collector.Records[0].Message.Should().Be($"outer {marker}");
    }

    [Fact]
    public void MinimumLevel_ShouldFilterPerSink_AndRuleLevelShouldBeTheLeastRestrictive()
    {
        var marker = NewMarker();
        var everything = new Collector(marker);
        var warningsOnly = new Collector(marker);

        var traceRegistration = BGLogger.Configuration.AddLiveSink(everything.Post, LogLevel.Trace);
        try
        {
            using (BGLogger.Configuration.AddLiveSink(warningsOnly.Post, LogLevel.Warn))
            {
                LiveRule()!.Levels.Should().Contain(LogLevel.Trace);

                BGLogger.Trace($"trace {marker}");
                BGLogger.Warn($"warn {marker}");

                everything.Records.Should().HaveCount(2);
                warningsOnly.Records.Should().ContainSingle();
                warningsOnly.Records[0].Level.Should().Be(LogLevel.Warn);

                traceRegistration.Dispose();

                var rule = LiveRule();
                rule.Should().NotBeNull();
                rule!.Levels.Should().NotContain(LogLevel.Trace);
                rule.Levels.Should().Contain(LogLevel.Warn);

                BGLogger.Trace($"ignored {marker}");
                warningsOnly.Records.Should().ContainSingle();
            }
        }
        finally
        {
            traceRegistration.Dispose();
        }
    }

    [Fact]
    public void MinimumLevel_MorePermissiveSinkAddedLater_ShouldWidenTheRuleLevel()
    {
        var marker = NewMarker();
        var warningsOnly = new Collector(marker);
        var everything = new Collector(marker);

        // The restrictive sink comes FIRST: the rule must be widened, not only ever raised.
        var warnRegistration = BGLogger.Configuration.AddLiveSink(warningsOnly.Post, LogLevel.Warn);
        try
        {
            LiveRule()!.Levels.Should().NotContain(LogLevel.Trace);

            using (BGLogger.Configuration.AddLiveSink(everything.Post, LogLevel.Trace))
            {
                LiveRule()!.Levels.Should().Contain(LogLevel.Trace);

                BGLogger.Trace($"widened {marker}");

                everything.Records.Should().ContainSingle();
                everything.Records[0].Level.Should().Be(LogLevel.Trace);
                warningsOnly.Records.Should().BeEmpty();
            }

            LiveRule()!.Levels.Should().NotContain(LogLevel.Trace);
        }
        finally
        {
            warnRegistration.Dispose();
        }
    }

    [Fact]
    public void AddLiveSink_WithoutMinimumLevel_ShouldCaptureFromTrace()
    {
        var marker = NewMarker();
        var collector = new Collector(marker);

        using (BGLogger.Configuration.AddLiveSink(collector.Post))
        {
            LiveRule()!.Levels.Should().Contain(LogLevel.Trace);

            BGLogger.Trace($"trace {marker}");
            BGLogger.Debug($"debug {marker}");
        }

        collector.Records.Select(record => record.Level).Should().Equal(LogLevel.Trace, LogLevel.Debug);
    }

    [Fact]
    public void AddLiveSink_FromInsideACallback_ShouldThrowInsteadOfDeadlocking()
    {
        var marker = NewMarker();
        Exception? captured = null;

        using (BGLogger.Configuration.AddLiveSink(_ =>
               {
                   try
                   {
                       BGLogger.Configuration.AddLiveSink(_ => { }).Dispose();
                   }
                   catch (Exception ex)
                   {
                       captured = ex;
                   }
               }))
        {
            BGLogger.Info($"nested registration {marker}");
        }

        captured.Should().BeOfType<InvalidOperationException>();
        BGLogger.Configuration.LiveSinkCount.Should().Be(0);
    }

    [Fact]
    public void Dispose_FromInsideACallback_ShouldThrowAndKeepTheRegistrationUsable()
    {
        var marker = NewMarker();
        var collector = new Collector(marker);
        Exception? captured = null;

        IDisposable? registration = null;
        registration = BGLogger.Configuration.AddLiveSink(record =>
        {
            collector.Post(record);

            try
            {
                // The natural one-shot sink. Taking the live lock from a logging thread that already holds
                // NLog's per-target lock deadlocks, so this has to fail fast instead.
                registration!.Dispose();
            }
            catch (Exception ex)
            {
                captured = ex;
            }
        });

        try
        {
            BGLogger.Info($"self dispose {marker}");

            captured.Should().BeOfType<InvalidOperationException>();
            collector.Records.Should().ContainSingle();

            // The failed in-callback dispose left the handle valid: disposing it here still unregisters.
            BGLogger.Configuration.LiveSinkCount.Should().Be(1);
        }
        finally
        {
            registration.Dispose();
        }

        BGLogger.Configuration.LiveSinkCount.Should().Be(0);
        LiveTarget().Should().BeNull();
    }

    [Fact]
    public void AddLiveSink_WhenReconfigurationFails_ShouldLeaveNoTargetOrRuleBehind()
    {
        // Initialize BGLogger BEFORE turning exceptions on: its static constructor builds the whole
        // configuration, and anything NLog would only warn about becomes a cached TypeInitializationException
        // that fails every later test in this collection instead of just this one.
        _ = BGLogger.Configuration;

        var throwExceptions = LogManager.ThrowExceptions;
        var broken = new FailingTarget { Name = "BROKEN" };
        var brokenRule = new LoggingRule("*", LogLevel.Fatal, broken);

        try
        {
            LogManager.ThrowExceptions = true;
            BGLogger.Configuration.Targets.AddTarget("BROKEN", broken);
            BGLogger.Configuration.Targets.LoggingRules.Add(brokenRule);

            Action act = () => BGLogger.Configuration.AddLiveSink(_ => { });

            act.Should().Throw<Exception>();

            // Without the rollback the LIVE target and its "*" rule survive the failure, and every log call
            // in the process keeps formatting a LogEventInfo for a target that has no sinks.
            BGLogger.Configuration.LiveSinkCount.Should().Be(0);
            LiveTarget().Should().BeNull();
            LiveRule().Should().BeNull();
        }
        finally
        {
            BGLogger.Configuration.Targets.LoggingRules.Remove(brokenRule);
            BGLogger.Configuration.Targets.RemoveTarget("BROKEN");
            LogManager.ThrowExceptions = throwExceptions;
            BGLogger.Configuration.Reconfigure();
        }
    }

    [Fact]
    public void LiveRule_AfterRemoval_ShouldStillHaveItsTarget()
    {
        var registration = BGLogger.Configuration.AddLiveSink(_ => { });
        try
        {
            var rule = LiveRule();
            rule.Should().NotBeNull();

            registration.Dispose();

            // The rule must be removed BEFORE the target: the reverse order permanently empties rule.Targets.
            rule!.Targets.Should().ContainSingle();
            rule.Targets[0].Name.Should().Be(LiveTargetName);
            LiveRule().Should().BeNull();
        }
        finally
        {
            registration.Dispose();
        }
    }

    [Fact]
    public void BGLogViewBuffer_WithoutSubscribeDelegate_ShouldAttachToBGLogger()
    {
        var marker = NewMarker();

        using (var buffer = new BGLogViewBuffer(10))
        {
            buffer.Attach();

            buffer.IsAttached.Should().BeTrue();
            BGLogger.Configuration.LiveSinkCount.Should().Be(1);

            BGLogger.Warn($"through the view buffer {marker}");

            var update = buffer.Drain();
            update.Added.Select(record => record.Message).Should().Contain($"through the view buffer {marker}");
            update.Added.Where(record => record.Message.Contains(marker, StringComparison.Ordinal))
                .Should().AllSatisfy(record => record.IsWarning.Should().BeTrue());

            buffer.Detach();

            BGLogger.Configuration.LiveSinkCount.Should().Be(0);
            LiveTarget().Should().BeNull();
        }
    }

    [Fact]
    public void ConcurrentAddAndDispose_WhileTwoThreadsLog_ShouldNotThrow_AndEndUnregistered()
    {
        var marker = NewMarker();
        var failures = new ConcurrentQueue<Exception>();
        var stop = false;

        var loggers = Enumerable.Range(0, 2)
            .Select(_ => new Thread(() =>
            {
                try
                {
                    while (!Volatile.Read(ref stop))
                        BGLogger.Trace($"concurrent {marker}");
                }
                catch (Exception ex)
                {
                    failures.Enqueue(ex);
                }
            })
            {
                IsBackground = true
            })
            .ToArray();

        foreach (var logger in loggers)
            logger.Start();

        try
        {
            for (var cycle = 0; cycle < 200; cycle++)
                BGLogger.Configuration.AddLiveSink(_ => { }).Dispose();
        }
        finally
        {
            Volatile.Write(ref stop, true);

            foreach (var logger in loggers)
                logger.Join();
        }

        failures.Should().BeEmpty();
        BGLogger.Configuration.LiveSinkCount.Should().Be(0);
        LiveTarget().Should().BeNull();
        LiveRule().Should().BeNull();
    }

    private static Target? LiveTarget()
    {
        _ = BGLogger.Configuration;
        return LogManager.Configuration?.FindTargetByName(LiveTargetName);
    }

    private static LoggingRule? LiveRule()
    {
        _ = BGLogger.Configuration;
        return LogManager.Configuration?.LoggingRules
            .FirstOrDefault(rule => rule.Targets.Any(target => target.Name == LiveTargetName));
    }

    private static string NewMarker() => Guid.NewGuid().ToString("N");

    /// <summary>Stands in for any mis-configured target (unwritable directory, bad endpoint, …).</summary>
    [Target("Failing")]
    private sealed class FailingTarget : TargetWithLayout
    {
        protected override void InitializeTarget() =>
            throw new InvalidOperationException("deliberate target failure");
    }

    /// <summary>Thread-safe sink that keeps only the records carrying its own marker.</summary>
    private sealed class Collector
    {
        private readonly List<BGLogRecord> _records = new();

        public Collector(string? marker = null) => Marker = marker ?? NewMarker();

        public string Marker { get; }

        public IReadOnlyList<BGLogRecord> Records
        {
            get { lock (_records) { return _records.ToArray(); } }
        }

        public int Count
        {
            get { lock (_records) { return _records.Count; } }
        }

        public void Post(BGLogRecord record)
        {
            if (!record.Message.Contains(Marker, StringComparison.Ordinal))
                return;

            lock (_records) { _records.Add(record); }
        }
    }
}
