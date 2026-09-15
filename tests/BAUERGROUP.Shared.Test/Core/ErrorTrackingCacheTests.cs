using BAUERGROUP.Shared.Core.Application;
using BAUERGROUP.Shared.Core.ErrorTracking;
using BAUERGROUP.Shared.Core.Logging;
using Sentry;

namespace BAUERGROUP.Shared.Test.Core;

public class ErrorTrackingCacheTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "BGErrorTrackingCacheTests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void Apply_WithDirectory_ShouldEnableCacheAndCreateFolder()
    {
        var options = new SentryOptions();
        var directory = Path.Combine(_root, "ErrorReports");

        ErrorTrackingCache.Apply(options, directory, 50, TimeSpan.Zero);

        options.CacheDirectoryPath.Should().Be(directory);
        options.MaxCacheItems.Should().Be(50);
        options.InitCacheFlushTimeout.Should().Be(TimeSpan.Zero);
        Directory.Exists(directory).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Apply_WithoutDirectory_ShouldDisableCache(string? directory)
    {
        var options = new SentryOptions { CacheDirectoryPath = "previous" };

        ErrorTrackingCache.Apply(options, directory, 50, TimeSpan.Zero);

        options.CacheDirectoryPath.Should().BeNull();
    }

    [Fact]
    public void Apply_WithUncreatableDirectory_ShouldDisableCacheInsteadOfThrowing()
    {
        // A directory below an existing file can never be created
        Directory.CreateDirectory(_root);
        var file = Path.Combine(_root, "not-a-directory");
        File.WriteAllText(file, string.Empty);
        var options = new SentryOptions();

        var action = () => ErrorTrackingCache.Apply(options, Path.Combine(file, "ErrorReports"), 50, TimeSpan.Zero);

        action.Should().NotThrow();
        options.CacheDirectoryPath.Should().BeNull();
    }

    [Fact]
    public void Defaults_ShouldKeep50ReportsInApplicationDataFolder()
    {
        var expectedDirectory = Path.Combine(ApplicationFolders.ExecutionAutomaticApplicationDataFolder, "ErrorReports");

        var trackingConfiguration = new BGErrorTrackingConfiguration();
        trackingConfiguration.CacheDirectoryPath.Should().Be(expectedDirectory);
        trackingConfiguration.MaxCacheItems.Should().Be(50);
        trackingConfiguration.InitCacheFlushTimeout.Should().Be(TimeSpan.Zero);

        BGLogger.Configuration.SentryCacheDirectoryPath.Should().Be(expectedDirectory);
        BGLogger.Configuration.SentryMaxCacheItems.Should().Be(50);
        BGLogger.Configuration.SentryInitCacheFlushTimeout.Should().Be(TimeSpan.Zero);
    }
}
