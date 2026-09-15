using System.Reflection;
using BAUERGROUP.Shared.Core.Application;
using BAUERGROUP.Shared.Core.Logging;

namespace BAUERGROUP.Shared.Test.Core;

// Single-file applications have an empty Assembly.Location; the IL3000 analyzer (error) guards against
// new Location uses at build time, these tests pin the replacement behaviour.
public class ApplicationFoldersTests
{
    private static string ExpectedName => Assembly.GetEntryAssembly()!.GetName().Name!;

    [Fact]
    public void ApplicationFileNameWithoutExtension_ShouldBeEntryAssemblyName()
    {
        ApplicationFolders.ApplicationFileNameWithoutExtension.Should().NotBeNullOrWhiteSpace()
            .And.Be(ExpectedName);
    }

    [Fact]
    public void ApplicationBinary_ShouldBeBaseDirectoryWithoutTrailingSeparator()
    {
        var expected = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        ApplicationFolders.ApplicationBinary.Should().Be(expected);
        ApplicationFolders.ApplicationExecuting.Should().Be(expected);
        Directory.Exists(ApplicationFolders.ApplicationBinary).Should().BeTrue();
    }

    [Fact]
    public void ExecutionCommonApplicationDataFolder_ShouldNotCollapseIntoParent()
    {
        ApplicationFolders.ExecutionCommonApplicationDataFolder.Should()
            .Be(Path.Combine(ApplicationFolders.CommonApplicationData, ExpectedName));
    }

    [Fact]
    public void BGLoggerConfiguration_ApplicationName_ShouldNotBeEmpty()
    {
        _ = BGLogger.Configuration;

        BGLoggerConfiguration.ApplicationName.Should().NotBeNullOrWhiteSpace();
    }
}
