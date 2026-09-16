using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using BAUERGROUP.Shared.Core.Logging;
using System;
using System.IO;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(BAUERGROUP.Shared.Avalonia.Test.TestAppBuilder))]

// BGLogger and NLog's LogManager are process-global: two viewers created in parallel would see each
// other's live-sink registrations.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace BAUERGROUP.Shared.Avalonia.Test
{
    /// <summary>Minimal application for the headless session. A theme is required: without one the ListBox
    /// has no control template, so nothing is realized and the ScrollViewer does not exist.</summary>
    public sealed class TestApp : Application
    {
        /// <inheritdoc />
        public override void Initialize()
        {
            Styles.Add(new FluentTheme());
        }
    }

    /// <summary>Entry point used by <c>AvaloniaTestApplication</c>.</summary>
    public static class TestAppBuilder
    {
        static TestAppBuilder()
        {
            // Redirect and disable file logging before BGLogger.Configuration is constructed, so CI runners
            // never depend on a writable data folder.
            BGLoggerConfiguration.LogDirectory =
                Path.Combine(Path.GetTempPath(), "BAUERGROUP.Shared.Avalonia.Test");
            BGLogger.Configuration.File = false;
        }

        /// <summary>Builds the headless Avalonia application for the test session.</summary>
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<TestApp>()
                             .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = true });
        }
    }
}
