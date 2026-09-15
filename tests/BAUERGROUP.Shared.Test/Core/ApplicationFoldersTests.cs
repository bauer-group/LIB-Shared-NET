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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ResolveAutomaticApplicationDataFolder_WithRoamingVariable_ShouldUseRoamingProfileOnAnyOs(bool isWindows)
    {
        var folder = ApplicationFolders.ResolveAutomaticApplicationDataFolder(isWindows, "true", "MyApp", _ => true);

        folder.Should().Be(Path.Combine(ApplicationFolders.ApplicationData, "MyApp"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("FALSE")]
    public void ResolveAutomaticApplicationDataFolder_OnWindows_ShouldUseCommonApplicationData(string? roamingVariable)
    {
        var folder = ApplicationFolders.ResolveAutomaticApplicationDataFolder(true, roamingVariable, "MyApp", _ => true);

        folder.Should().Be(Path.Combine(ApplicationFolders.CommonApplicationData, "MyApp"));
    }

    [Fact]
    public void ResolveAutomaticApplicationDataFolder_OnUnix_ShouldUseVarLibWhenItExists()
    {
        var expected = Path.Combine("/var/lib", "MyApp");

        var folder = ApplicationFolders.ResolveAutomaticApplicationDataFolder(false, null, "MyApp", path => path == expected);

        folder.Should().Be(expected);
    }

    [Fact]
    public void ResolveAutomaticApplicationDataFolder_OnUnix_ShouldFallBackToPerUserFolder()
    {
        var folder = ApplicationFolders.ResolveAutomaticApplicationDataFolder(false, null, "MyApp", _ => false);

        var localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
        folder.Should().Be(Path.Combine(localApplicationData, "MyApp"));
        folder.Should().NotStartWith(ApplicationFolders.CommonApplicationData);
    }

    [Fact]
    public void BGLoggerConfiguration_ApplicationName_ShouldNotBeEmpty()
    {
        _ = BGLogger.Configuration;

        BGLoggerConfiguration.ApplicationName.Should().NotBeNullOrWhiteSpace();
    }
}
