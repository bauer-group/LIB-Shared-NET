using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using BAUERGROUP.Shared.Avalonia.Logging;
using BAUERGROUP.Shared.Core.Logging;
using NLog;
using System;
using System.Linq;

namespace BAUERGROUP.Shared.Avalonia.Test.Logging
{
    /// <summary>
    /// Attach and detach behaviour against the real BGLogger live sink: a viewer owns exactly one
    /// registration while it is in a visual tree and nothing afterwards.
    /// </summary>
    public class LogViewerLifecycleTests
    {
        [AvaloniaFact]
        public void Show_ShouldRegisterOneLiveSink_Close_ShouldLeaveNothingRegistered()
        {
            var baseline = BGLogger.Configuration.LiveSinkCount;
            var viewer = new LogViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                BGLogger.Configuration.LiveSinkCount.Should().Be(baseline + 1);
                BGLogger.Configuration.Targets.FindTargetByName("LIVE").Should().NotBeNull();
            }
            finally
            {
                window.Close();
            }

            BGLogger.Configuration.LiveSinkCount.Should().Be(baseline);
            BGLogger.Configuration.Targets.FindTargetByName("LIVE").Should().BeNull();
        }

        [AvaloniaFact]
        public void ContentNullAndBack_ShouldAttachIdempotently()
        {
            var baseline = BGLogger.Configuration.LiveSinkCount;
            var viewer = new LogViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                BGLogger.Configuration.LiveSinkCount.Should().Be(baseline + 1);

                window.Content = null;
                BGLogger.Configuration.LiveSinkCount.Should().Be(baseline, "detaching removes the registration");

                window.Content = viewer;
                window.UpdateLayout();
                BGLogger.Configuration.LiveSinkCount.Should().Be(baseline + 1, "re-attaching registers exactly once");

                // Repeated cycles must never accumulate registrations.
                window.Content = null;
                window.Content = viewer;
                window.UpdateLayout();
                BGLogger.Configuration.LiveSinkCount.Should().Be(baseline + 1);
            }
            finally
            {
                window.Close();
            }

            BGLogger.Configuration.LiveSinkCount.Should().Be(baseline);
        }

        [AvaloniaFact]
        public void TwoViewers_ShouldBeReferenceCounted()
        {
            var baseline = BGLogger.Configuration.LiveSinkCount;
            var marker = Guid.NewGuid().ToString("N");
            var first = new LogViewer();
            var second = new LogViewer();
            var firstWindow = TestHelpers.Show(first);
            var secondWindow = TestHelpers.Show(second);

            try
            {
                BGLogger.Configuration.LiveSinkCount.Should().Be(baseline + 2);

                firstWindow.Close();

                BGLogger.Configuration.LiveSinkCount.Should().Be(baseline + 1);
                BGLogger.Configuration.Targets.FindTargetByName("LIVE")
                        .Should().NotBeNull("the target stays while another viewer is open");

                // The live sink delivers synchronously on the logging thread, so the record is already in the
                // pending buffer here; Refresh applies it without depending on the update timer.
                BGLogger.Info("still alive " + marker);
                second.Refresh();

                second.GetText().Should().Contain(marker);
            }
            finally
            {
                firstWindow.Close();
                secondWindow.Close();
            }

            BGLogger.Configuration.LiveSinkCount.Should().Be(baseline);
        }

        [AvaloniaFact]
        public void DetachedViewer_ShouldNotReceivePostedRecords()
        {
            var baseline = BGLogger.Configuration.LiveSinkCount;
            var marker = Guid.NewGuid().ToString("N");
            var viewer = new LogViewer();
            var window = TestHelpers.Show(viewer);

            window.Close();
            BGLogger.Configuration.LiveSinkCount.Should().Be(baseline);

            BGLogger.Warn("after close " + marker);
            TestHelpers.Pump(300);

            viewer.LineCount.Should().Be(0, "the update timer is stopped while detached");

            // Nothing was captured at all, so an explicit refresh finds nothing either.
            viewer.Refresh();
            viewer.LineCount.Should().Be(0);
            viewer.GetText().Should().BeEmpty();
        }

        [AvaloniaFact]
        public void HiddenViewer_ShouldNotRegisterALiveSink()
        {
            var baseline = BGLogger.Configuration.LiveSinkCount;
            var marker = Guid.NewGuid().ToString("N");

            // The natural diagnostics-pane markup: <LogViewer IsVisible="{Binding ShowDiagnostics}" />
            // with the toggle off. The pane is in the visual tree from the start and must cost nothing.
            var viewer = new LogViewer { IsVisible = false };
            var window = TestHelpers.Show(viewer);

            try
            {
                viewer.GetVisualAncestors()
                      .Should().Contain(window, "a hidden control still is in the visual tree");

                BGLogger.Configuration.LiveSinkCount
                        .Should().Be(baseline, "a hidden pane must not capture for the whole process life");

                BGLogger.Info("while hidden " + marker);
                viewer.Refresh();

                viewer.LineCount.Should().Be(0);
                viewer.GetText().Should().NotContain(marker);
            }
            finally
            {
                window.Close();
            }

            BGLogger.Configuration.LiveSinkCount.Should().Be(baseline);
        }

        [AvaloniaFact]
        public void HidingAndShowingAgain_ShouldUnregisterAndReregister()
        {
            var baseline = BGLogger.Configuration.LiveSinkCount;
            var hiddenMarker = Guid.NewGuid().ToString("N");
            var visibleMarker = Guid.NewGuid().ToString("N");
            var viewer = new LogViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                BGLogger.Configuration.LiveSinkCount.Should().Be(baseline + 1);

                viewer.IsVisible = false;

                BGLogger.Configuration.LiveSinkCount
                        .Should().Be(baseline, "hiding an open viewer releases the registration");

                BGLogger.Info("hidden " + hiddenMarker);
                viewer.Refresh();
                viewer.GetText().Should().NotContain(hiddenMarker);

                viewer.IsVisible = true;
                window.UpdateLayout();

                BGLogger.Configuration.LiveSinkCount
                        .Should().Be(baseline + 1, "showing it again registers exactly once");

                BGLogger.Info("visible " + visibleMarker);
                viewer.Refresh();
                viewer.GetText().Should().Contain(visibleMarker);
            }
            finally
            {
                window.Close();
            }

            BGLogger.Configuration.LiveSinkCount.Should().Be(baseline);
        }

        [AvaloniaFact]
        public void Detach_ShouldResetThePendingCount()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            for (var i = 0; i < 200; i++)
                viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

            viewer.Refresh();
            TestHelpers.Settle(window);

            var scrollViewer = TestHelpers.ScrollViewerOf(TestHelpers.ListOf(viewer));
            scrollViewer.Offset = new Vector(0, 0);
            TestHelpers.Settle(window);

            viewer.IsPaused.Should().BeTrue();

            for (var i = 200; i < 250; i++)
                viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

            viewer.Refresh();
            viewer.PendingCount.Should().Be(50);

            window.Close();

            viewer.PendingCount.Should().Be(0, "detaching discards the pending records, counters included");
            viewer.LineCount.Should().Be(200, "the lines already shown are kept");
        }

        [AvaloniaFact]
        public void BGLoggerWarn_ShouldAppearWithWarnClass()
        {
            var marker = Guid.NewGuid().ToString("N");
            var viewer = new LogViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                BGLogger.Warn("warning " + marker);
                viewer.Refresh();
                TestHelpers.Settle(window);

                viewer.GetText().Should().Contain(marker);

                var list = TestHelpers.ListOf(viewer);
                var row = list.GetVisualDescendants()
                              .OfType<TextBlock>()
                              .First(t => t.Text != null && t.Text.Contains(marker, StringComparison.Ordinal));

                row.Classes.Should().Contain("warn");
                row.Classes.Should().NotContain("error");
                ((ISolidColorBrush)row.Foreground!).Color.Should().Be(Color.Parse("#9D5D00"));
            }
            finally
            {
                window.Close();
            }
        }
    }
}
