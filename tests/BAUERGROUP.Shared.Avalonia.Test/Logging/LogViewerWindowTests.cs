using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using BAUERGROUP.Shared.Avalonia.Logging;
using BAUERGROUP.Shared.Core.Logging;
using NLog;
using System;
using System.Linq;

namespace BAUERGROUP.Shared.Avalonia.Test.Logging
{
    /// <summary>Owner-based window semantics: no Topmost, no Position, no Activate, no static state.</summary>
    public class LogViewerWindowTests
    {
        [AvaloniaFact]
        public void ShowWithOwner_ShouldSetOwner_AndAppearInOwnedWindows()
        {
            var owner = new Window { Width = 400, Height = 300 };
            owner.Show();

            var logWindow = new LogViewerWindow();

            try
            {
                logWindow.Show(owner);

                logWindow.Owner.Should().BeSameAs(owner);
                owner.OwnedWindows.Should().Contain(logWindow);
                logWindow.Title.Should().Be("Log Viewer");
                logWindow.Viewer.Should().NotBeNull();
                logWindow.WindowStartupLocation.Should().Be(WindowStartupLocation.CenterOwner);
                logWindow.Topmost.Should().BeFalse("Wayland ignores Topmost, so the window never sets it");
            }
            finally
            {
                owner.Close();
            }
        }

        [AvaloniaFact]
        public void ClosingTheOwner_ShouldCloseTheLogWindowAndUnregisterTheSink()
        {
            var baseline = BGLogger.Configuration.LiveSinkCount;
            var owner = new Window { Width = 400, Height = 300 };
            owner.Show();

            var logWindow = new LogViewerWindow();
            var closed = false;
            logWindow.Closed += (_, _) => closed = true;

            try
            {
                logWindow.Show(owner);
                BGLogger.Configuration.LiveSinkCount.Should().Be(baseline + 1);

                owner.Close();

                closed.Should().BeTrue("closing the owner closes the windows it owns");
                BGLogger.Configuration.LiveSinkCount.Should().Be(baseline);
                BGLogger.Configuration.Targets.FindTargetByName("LIVE").Should().BeNull();
            }
            finally
            {
                logWindow.Close();
                owner.Close();
            }
        }

        [AvaloniaFact]
        public void Toggle_ShouldOpenThenClose_OneInstancePerOwner()
        {
            var baseline = BGLogger.Configuration.LiveSinkCount;
            var owner = new Window { Width = 400, Height = 300 };
            owner.Show();

            try
            {
                var opened = LogViewerWindow.Toggle(owner, "Diagnostics");

                opened.Should().NotBeNull();
                opened!.Title.Should().Be("Diagnostics");
                owner.OwnedWindows.OfType<LogViewerWindow>().Should().ContainSingle();
                BGLogger.Configuration.LiveSinkCount.Should().Be(baseline + 1);

                LogViewerWindow.Toggle(owner).Should().BeNull("the second call closes the open window");

                owner.OwnedWindows.OfType<LogViewerWindow>().Should().BeEmpty();
                BGLogger.Configuration.LiveSinkCount.Should().Be(baseline);

                // A third call opens a fresh one again - no static state survives the close.
                var reopened = LogViewerWindow.Toggle(owner);
                reopened.Should().NotBeNull();
                reopened.Should().NotBeSameAs(opened);
                LogViewerWindow.Toggle(owner).Should().BeNull();

                Assert.Throws<ArgumentNullException>(() => LogViewerWindow.Toggle(null!));
            }
            finally
            {
                owner.Close();
            }

            BGLogger.Configuration.LiveSinkCount.Should().Be(baseline);
        }

        [AvaloniaFact]
        public void Escape_ShouldClose()
        {
            var owner = new Window { Width = 400, Height = 300 };
            owner.Show();

            var logWindow = new LogViewerWindow();
            var closed = false;
            logWindow.Closed += (_, _) => closed = true;

            try
            {
                logWindow.Show(owner);

                var args = new KeyEventArgs
                {
                    RoutedEvent = InputElement.KeyDownEvent,
                    Key = Key.Escape,
                };

                logWindow.RaiseEvent(args);

                args.Handled.Should().BeTrue();
                closed.Should().BeTrue();
                owner.OwnedWindows.OfType<LogViewerWindow>().Should().BeEmpty();
            }
            finally
            {
                owner.Close();
            }
        }

        [AvaloniaFact]
        public void Viewer_MaxLinesAndMinimumLevel_ShouldBeConfigurableThroughTheWindow()
        {
            var baseline = BGLogger.Configuration.LiveSinkCount;
            var marker = Guid.NewGuid().ToString("N");
            var owner = new Window { Width = 400, Height = 300 };
            owner.Show();

            var logWindow = new LogViewerWindow("Diagnostics");
            logWindow.Viewer.MaxLines = 250;
            logWindow.Viewer.MinimumLevel = LogLevel.Warn;

            try
            {
                logWindow.Show(owner);

                logWindow.Viewer.MaxLines.Should().Be(250);
                logWindow.Viewer.MinimumLevel.Should().Be(LogLevel.Warn);
                BGLogger.Configuration.LiveSinkCount.Should().Be(baseline + 1);

                // The live sink delivers synchronously on the logging thread; Refresh applies what it
                // captured without depending on the update timer.
                BGLogger.Info("info " + marker);
                BGLogger.Warn("warn " + marker);
                logWindow.Viewer.Refresh();

                var text = logWindow.Viewer.GetText();
                text.Should().Contain("warn " + marker);
                text.Should().NotContain("info " + marker, "the capture level is Warn");

                logWindow.Viewer.Clear();
                logWindow.Viewer.LineCount.Should().Be(0);

                for (var i = 0; i < 300; i++)
                    logWindow.Viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                logWindow.Viewer.Refresh();

                logWindow.Viewer.LineCount.Should().Be(250, "MaxLines is forwarded to the buffer");
            }
            finally
            {
                owner.Close();
            }

            BGLogger.Configuration.LiveSinkCount.Should().Be(baseline);
        }
    }
}
