using System.Text;
using BAUERGROUP.Shared.Core.Logging;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace BAUERGROUP.Shared.Test.Core;

/// <summary>
/// Regression tests for issue #138: every boolean target property added and removed one shared
/// <see cref="LoggingRule"/> instance, so enabling twice and disabling once left a rule in the
/// configuration whose <c>Targets</c> collection had been emptied by <c>RemoveTarget</c>. Re-enabling
/// put that empty rule back and the target never logged again for the rest of the process, while the
/// getter kept reporting <c>true</c>.
/// </summary>
/// <remarks>
/// <c>LogManager</c> is process-global, so these tests share the <c>BGLogger</c> collection with every
/// other test that touches it and restore the original state in a <c>finally</c>.
/// </remarks>
[Collection("BGLogger")]
public class BGLoggerConfigurationTargetToggleTests
{
    private static BGLoggerConfiguration Configuration => BGLogger.Configuration;

    private static LoggingConfiguration Config
    {
        get
        {
            _ = BGLogger.Configuration;
            return LogManager.Configuration!;
        }
    }

    // FILE is on by default, MEMORY / CONSOLE / DEBUGGER are off by default.
    private static (Func<bool> Get, Action<bool> Set) Toggle(string targetName) => targetName switch
    {
        "FILE" => (() => Configuration.File, value => Configuration.File = value),
        "MEMORY" => (() => Configuration.Memory, value => Configuration.Memory = value),
        "CONSOLE" => (() => Configuration.Console, value => Configuration.Console = value),
        "DEBUGGER" => (() => Configuration.Debugger, value => Configuration.Debugger = value),
        _ => throw new ArgumentOutOfRangeException(nameof(targetName), targetName, "Unknown target."),
    };

    /// <summary>
    /// Counts the target across all rules rather than counting the rules that mention it: a rule listing
    /// the same target twice makes NLog write every event twice, and counting rules would not see it.
    /// </summary>
    private static int TargetOccurrences(Target target) =>
        Config.LoggingRules.Sum(rule => rule.Targets.Count(candidate => ReferenceEquals(candidate, target)));

    [Theory]
    [InlineData("FILE")]
    [InlineData("MEMORY")]
    [InlineData("CONSOLE")]
    [InlineData("DEBUGGER")]
    public void Toggle_EnableEnableDisableEnable_ShouldKeepExactlyOneRuleWithTheTarget(string targetName)
    {
        var (get, set) = Toggle(targetName);
        var original = get();
        var rulesBefore = Config.LoggingRules.Count;

        try
        {
            set(true);
            var target = Config.FindTargetByName(targetName);
            target.Should().NotBeNull();
            get().Should().BeTrue();
            TargetOccurrences(target!).Should().Be(1);

            // The second enable must be a no-op instead of adding the same rule object a second time.
            set(true);
            get().Should().BeTrue();
            Config.FindTargetByName(targetName).Should().BeSameAs(target);
            TargetOccurrences(target!).Should().Be(1);

            set(false);
            get().Should().BeFalse();
            Config.FindTargetByName(targetName).Should().BeNull();
            TargetOccurrences(target!).Should().Be(0);

            // The regression: this used to re-add a rule whose Targets collection was empty.
            set(true);
            get().Should().BeTrue();
            Config.FindTargetByName(targetName).Should().BeSameAs(target);
            TargetOccurrences(target!).Should().Be(1);
        }
        finally
        {
            set(original);
        }

        get().Should().Be(original);
        Config.LoggingRules.Count.Should().Be(rulesBefore);
        Config.LoggingRules.Should().OnlyContain(rule => rule.Targets.Count > 0);
    }

    [Theory]
    [InlineData("FILE")]
    [InlineData("MEMORY")]
    [InlineData("CONSOLE")]
    [InlineData("DEBUGGER")]
    public void Toggle_DisableTwice_ShouldBeIdempotent(string targetName)
    {
        var (get, set) = Toggle(targetName);
        var original = get();
        var rulesBefore = Config.LoggingRules.Count;

        // Absolute expectation instead of a count read back out of the subject: a target that starts
        // enabled already contributes its rule to rulesBefore, every other one does not.
        var rulesWhileDisabled = original ? rulesBefore - 1 : rulesBefore;

        try
        {
            set(true);
            var target = Config.FindTargetByName(targetName);
            target.Should().NotBeNull();

            set(false);
            get().Should().BeFalse();
            Config.FindTargetByName(targetName).Should().BeNull();
            TargetOccurrences(target!).Should().Be(0);
            Config.LoggingRules.Count.Should().Be(rulesWhileDisabled);

            set(false);
            get().Should().BeFalse();
            Config.FindTargetByName(targetName).Should().BeNull();
            TargetOccurrences(target!).Should().Be(0);
            Config.LoggingRules.Count.Should().Be(rulesWhileDisabled);
        }
        finally
        {
            set(original);
        }

        Config.LoggingRules.Count.Should().Be(rulesBefore);
        Config.LoggingRules.Should().OnlyContain(rule => rule.Targets.Count > 0);
    }

    [Fact]
    public void Toggle_RepeatedEnabling_ShouldNotAccumulateLoggingRules()
    {
        var original = Configuration.Console;
        var rulesBefore = Config.LoggingRules.Count;

        try
        {
            for (var attempt = 0; attempt < 5; attempt++)
                Configuration.Console = true;

            Config.LoggingRules.Count.Should().Be(original ? rulesBefore : rulesBefore + 1);
        }
        finally
        {
            Configuration.Console = original;
        }

        Config.LoggingRules.Count.Should().Be(rulesBefore);
    }

    /// <summary>
    /// <c>Targets</c> is public, so an application can unregister a target behind the property's back.
    /// NLog empties the rule but leaves it in the list - the one state in which the two halves of the
    /// guard disagree, and the only one that exercises the repair branch of <c>ApplyTarget</c>.
    /// </summary>
    [Fact]
    public void Toggle_AfterExternalRemoveTarget_ShouldRepairTheRuleInsteadOfDuplicatingIt()
    {
        var original = Configuration.Memory;
        Configuration.Memory = false;
        var rulesDisabled = Config.LoggingRules.Count;

        try
        {
            Configuration.Memory = true;
            var target = Config.FindTargetByName("MEMORY");
            target.Should().NotBeNull();
            Config.LoggingRules.Count.Should().Be(rulesDisabled + 1);

            RemoveTargetBehindThePropertysBack();
            Configuration.Memory.Should().BeFalse();
            TargetOccurrences(target!).Should().Be(0);

            // Enabling repairs the rule that is still in the list instead of appending it a second time.
            Configuration.Memory = true;
            Configuration.Memory.Should().BeTrue();
            Config.LoggingRules.Count.Should().Be(rulesDisabled + 1);
            TargetOccurrences(target!).Should().Be(1);
            Config.LoggingRules.Should().OnlyContain(rule => rule.Targets.Count > 0);

            var marker = $"issue138-heal-{Guid.NewGuid():N}";
            Configuration.MemoryLogsClear();
            BGLogger.Info(marker);
            Configuration.MemoryLogs.Count(entry => entry.Contains(marker)).Should().Be(1);

            // Disabling drops the emptied rule instead of short-circuiting on the getter.
            RemoveTargetBehindThePropertysBack();
            Configuration.Memory = false;
            Config.LoggingRules.Count.Should().Be(rulesDisabled);
            Config.LoggingRules.Should().OnlyContain(rule => rule.Targets.Count > 0);
        }
        finally
        {
            Configuration.MemoryLogsClear();
            Configuration.Memory = original;
        }
    }

    [Fact]
    public void Memory_AfterEnableEnableDisableEnable_ShouldStillReceiveLogEvents()
    {
        var original = Configuration.Memory;

        try
        {
            Configuration.Memory = true;
            Configuration.Memory = true;
            Configuration.Memory = false;
            Configuration.Memory = true;

            Configuration.Memory.Should().BeTrue();

            var marker = $"issue138-memory-{Guid.NewGuid():N}";
            Configuration.MemoryLogsClear();
            BGLogger.Info(marker);

            // Exactly once: a duplicated rule or a rule listing the target twice would log it twice.
            Configuration.MemoryLogs.Count(entry => entry.Contains(marker)).Should().Be(1);
        }
        finally
        {
            Configuration.MemoryLogsClear();
            Configuration.Memory = original;
        }
    }

    [Fact]
    public void File_AfterEnableEnableDisableEnable_ShouldStillWriteToTheLogFile()
    {
        var original = Configuration.File;
        var originalDirectory = BGLoggerConfiguration.LogDirectory;
        var directory = Path.Combine(Path.GetTempPath(), "BGLoggerTargetToggleTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        try
        {
            // The FileTarget renders its path from the LogDirectory GDC item on every write, so the test
            // can redirect the real target into a temporary folder instead of writing to the machine log.
            BGLoggerConfiguration.LogDirectory = directory;

            Configuration.File = true;
            Configuration.File = true;
            Configuration.File = false;
            Configuration.File = true;

            Configuration.File.Should().BeTrue();

            var marker = $"issue138-file-{Guid.NewGuid():N}";
            BGLogger.Info(marker);
            LogManager.Flush();

            Occurrences(ReadLogFile(directory), marker).Should().Be(1);
        }
        finally
        {
            // Disable first: the target keeps the file open, and the handle must be gone before the
            // temporary folder can be deleted.
            Configuration.File = false;
            LogManager.Flush();
            BGLoggerConfiguration.LogDirectory = originalDirectory;
            Configuration.File = original;

            try
            {
                Directory.Delete(directory, recursive: true);
            }
            catch (IOException)
            {
                // Best effort - a leftover temporary folder must not fail the test.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    [Fact]
    public void ErrorTracking_EnabledWithoutDsn_ShouldStillThrowAndStayDisabled()
    {
        var originalDsn = Configuration.SentryDsn;

        try
        {
            Configuration.SentryDsn = null;

            Action act = () => Configuration.ErrorTracking = true;

            act.Should().Throw<InvalidOperationException>();
            Configuration.ErrorTracking.Should().BeFalse();
            Config.FindTargetByName("ERRORTRACKING").Should().BeNull();
        }
        finally
        {
            Configuration.SentryDsn = originalDsn;
        }
    }

    private static void RemoveTargetBehindThePropertysBack()
    {
        Configuration.Targets.RemoveTarget("MEMORY");
        Configuration.Reconfigure();
    }

    private static int Occurrences(string haystack, string needle)
    {
        var count = 0;

        for (var index = haystack.IndexOf(needle, StringComparison.Ordinal);
             index >= 0;
             index = haystack.IndexOf(needle, index + needle.Length, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
    }

    private static string ReadLogFile(string directory)
    {
        var path = Path.Combine(directory, $"{BGLoggerConfiguration.ApplicationName}.log");
        if (!File.Exists(path))
            return string.Empty;

        // KeepFileOpen is on, so the file must be opened with sharing.
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream, Encoding.Unicode);
        return reader.ReadToEnd();
    }
}
