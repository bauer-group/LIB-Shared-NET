using System.Globalization;
using BAUERGROUP.Shared.Core.Logging;
using NLog;
using NLog.Layouts;
using NLog.Targets;

namespace BAUERGROUP.Shared.Test.Core;

[Collection("BGLogger")]
public class BGLoggerConfigurationTests
{
    private static FileTarget GetFileTarget()
    {
        _ = BGLogger.Configuration;
        return LogManager.Configuration!.FindTargetByName<FileTarget>("FILE")!;
    }

    [Fact]
    public void FileTarget_Paths_ShouldNotContainHardCodedBackslash()
    {
        var target = GetFileTarget();

        target.FileName.ToString().Should().NotContain("\\");
        target.ArchiveFileName!.ToString().Should().NotContain("\\");
    }

    [Fact]
    public void FileTarget_RenderedFileName_ShouldBeInsideLogDirectory()
    {
        var target = GetFileTarget();

        var rendered = target.FileName.Render(LogEventInfo.CreateNullEvent());

        Path.GetDirectoryName(rendered).Should().Be(BGLoggerConfiguration.LogDirectory);
        Path.GetFileName(rendered).Should().Be($"{BGLoggerConfiguration.ApplicationName}.log");
    }

    [Fact]
    public void FileTarget_ArchiveSuffix_ShouldContainArchiveDate()
    {
        var target = GetFileTarget();

        Suffix(target, new DateTime(2026, 9, 14)).Should().Contain("2026-09-14");
    }

    [Fact]
    public void FileTarget_ArchiveSuffix_ShouldDifferPerDay()
    {
        var target = GetFileTarget();

        Suffix(target, new DateTime(2026, 9, 15)).Should().NotBe(Suffix(target, new DateTime(2026, 9, 14)));
    }

    [Fact]
    public void FileTarget_ArchiveSuffix_ShouldContainSequenceNumber()
    {
        var target = GetFileTarget();

        // Without {0} a second archive of the same day is appended to the existing file
        target.ArchiveSuffixFormat.Should().Contain("{0");
    }

    [Fact]
    public void FileTarget_Layout_ShouldParseWhileNLogThrowsConfigurationExceptions()
    {
        var target = GetFileTarget();
        var throwExceptions = LogManager.ThrowExceptions;

        try
        {
            // A doubled colon after a renderer name ("${longdate::universalTime=true}") renders fine but is
            // rejected outright once an application turns exceptions on. Every other target reuses this
            // layout, so the parse happens inside InitializeTargets - that is, inside the BGLogger static
            // constructor, whose TypeInitializationException is cached: logging is then dead process-wide.
            LogManager.ThrowExceptions = true;

            Action act = () => Layout.FromString(target.Layout.ToString()!);

            act.Should().NotThrow();
        }
        finally
        {
            LogManager.ThrowExceptions = throwExceptions;
        }
    }

    // NLog 6 formats the suffix as string.Format(format, sequenceNumber, archiveDate)
    private static string Suffix(FileTarget target, DateTime day) =>
        string.Format(CultureInfo.InvariantCulture, target.ArchiveSuffixFormat, 0, day);
}
