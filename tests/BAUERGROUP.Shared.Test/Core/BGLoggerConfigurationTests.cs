using BAUERGROUP.Shared.Core.Logging;
using NLog;
using NLog.Targets;

namespace BAUERGROUP.Shared.Test.Core;

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
}
