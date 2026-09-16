using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using NLog;
using System;

namespace BAUERGROUP.Shared.Avalonia.Test.Logging
{
    /// <summary>
    /// Follow-the-newest-line behaviour and the scroll-up pause, including the steady state at capacity that
    /// must never pause itself.
    /// </summary>
    public class LogViewerScrollTests
    {
        [AvaloniaFact]
        public void AutoScroll_ShouldReachTheEndOnTheFirstBatchAndOnLaterBatches()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 200; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                TestHelpers.Settle(window);

                var list = TestHelpers.ListOf(viewer);
                var scrollViewer = TestHelpers.ScrollViewerOf(list);

                TestHelpers.DistanceToBottom(scrollViewer)
                           .Should().BeLessThan(2, "the very first batch already scrolls to the end");
                list.ContainerFromIndex(viewer.LineCount - 1).Should().NotBeNull();

                for (var i = 200; i < 300; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                TestHelpers.Settle(window);

                TestHelpers.DistanceToBottom(scrollViewer).Should().BeLessThan(2);
                list.ContainerFromIndex(viewer.LineCount - 1).Should().NotBeNull();
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void SteadyStateAtCapacity_ShouldNotSelfPause()
        {
            var viewer = TestHelpers.IsolatedViewer(200);
            var window = TestHelpers.Show(viewer);

            try
            {
                var index = 0;

                for (var round = 0; round < 20; round++)
                {
                    for (var i = 0; i < 50; i++)
                    {
                        // Every fifth row carries multi-line exception text, so row heights vary and the
                        // extent changes on every batch.
                        viewer.Post(index % 5 == 0
                            ? TestHelpers.RecordWithException(LogLevel.Error, "line " + index)
                            : TestHelpers.Record(LogLevel.Info, "line " + index));

                        index++;
                    }

                    viewer.Refresh();
                    TestHelpers.Settle(window);

                    viewer.IsPaused.Should().BeFalse("round {0} must not pause the view by itself", round);
                }

                var list = TestHelpers.ListOf(viewer);

                viewer.LineCount.Should().Be(200);
                list.ContainerFromIndex(viewer.LineCount - 1)
                    .Should().NotBeNull("the newest line stays realized while following");
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void UserScrollUp_ShouldPauseAndFreezeTheView()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 200; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                TestHelpers.Settle(window);

                var list = TestHelpers.ListOf(viewer);
                var scrollViewer = TestHelpers.ScrollViewerOf(list);

                scrollViewer.Offset = new Vector(0, 0);
                TestHelpers.Settle(window);

                viewer.IsPaused.Should().BeTrue();

                var frozen = viewer.LineCount;

                for (var i = 200; i < 250; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                TestHelpers.Pump(300);
                TestHelpers.Settle(window);

                viewer.LineCount.Should().Be(frozen, "the view is frozen while the user has scrolled up");
                viewer.PendingCount.Should().Be(50);
                scrollViewer.Offset.Y.Should().Be(0);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void ScrollToNewest_ShouldResumeAndAppendPending()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 200; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                TestHelpers.Settle(window);

                var list = TestHelpers.ListOf(viewer);
                var scrollViewer = TestHelpers.ScrollViewerOf(list);

                scrollViewer.Offset = new Vector(0, 0);
                TestHelpers.Settle(window);
                viewer.IsPaused.Should().BeTrue();

                for (var i = 200; i < 250; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.ScrollToNewest();
                TestHelpers.Settle(window);

                viewer.IsPaused.Should().BeFalse();
                viewer.LineCount.Should().Be(250);
                viewer.PendingCount.Should().Be(0);
                TestHelpers.DistanceToBottom(scrollViewer).Should().BeLessThan(2);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void ScrollBackToBottom_ShouldResume()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 200; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                TestHelpers.Settle(window);

                var list = TestHelpers.ListOf(viewer);
                var scrollViewer = TestHelpers.ScrollViewerOf(list);

                scrollViewer.Offset = new Vector(0, 0);
                TestHelpers.Settle(window);
                viewer.IsPaused.Should().BeTrue();

                scrollViewer.ScrollToEnd();
                TestHelpers.Settle(window);

                viewer.IsPaused.Should().BeFalse("scrolling back to the end resumes following");

                for (var i = 200; i < 220; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                TestHelpers.Settle(window);

                viewer.LineCount.Should().Be(220);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void EndKey_ShouldResumeAndJumpToTheNewestLine()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 200; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                TestHelpers.Settle(window);

                var scrollViewer = TestHelpers.ScrollViewerOf(TestHelpers.ListOf(viewer));

                scrollViewer.Offset = new Vector(0, 0);
                TestHelpers.Settle(window);
                viewer.IsPaused.Should().BeTrue();

                var args = new KeyEventArgs
                {
                    RoutedEvent = InputElement.KeyDownEvent,
                    Key = Key.End,
                };

                viewer.RaiseEvent(args);
                TestHelpers.Settle(window);

                args.Handled.Should().BeTrue();
                viewer.IsPaused.Should().BeFalse();
                TestHelpers.DistanceToBottom(scrollViewer).Should().BeLessThan(2);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void ScrollToNewest_WithAutoScrollFalse_ShouldStillJumpToTheEnd()
        {
            var viewer = TestHelpers.IsolatedViewer();
            viewer.AutoScroll = false;
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 200; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                TestHelpers.Settle(window);

                var scrollViewer = TestHelpers.ScrollViewerOf(TestHelpers.ListOf(viewer));

                scrollViewer.Offset.Y.Should().Be(0, "a view that does not follow never scrolls by itself");
                TestHelpers.DistanceToBottom(scrollViewer).Should().BeGreaterThan(2);

                viewer.ScrollToNewest();
                TestHelpers.Settle(window);

                TestHelpers.DistanceToBottom(scrollViewer)
                           .Should().BeLessThan(2, "an explicit jump to the end is not the automatic follow path");

                // The explicit jump must not turn following back on behind the consumer's back.
                viewer.AutoScroll.Should().BeFalse();
                viewer.IsPaused.Should().BeFalse();

                for (var i = 200; i < 250; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                TestHelpers.Settle(window);

                TestHelpers.DistanceToBottom(scrollViewer)
                           .Should().BeGreaterThan(2, "later batches still do not scroll by themselves");
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void AutoScrollFalse_ShouldNeverPauseAndNeverScroll()
        {
            var viewer = TestHelpers.IsolatedViewer();
            viewer.AutoScroll = false;
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 200; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                TestHelpers.Settle(window);

                var list = TestHelpers.ListOf(viewer);
                var scrollViewer = TestHelpers.ScrollViewerOf(list);

                viewer.LineCount.Should().Be(200);
                scrollViewer.Offset.Y.Should().Be(0, "a view that does not follow never scrolls by itself");
                viewer.IsPaused.Should().BeFalse();

                scrollViewer.Offset = new Vector(0, 50);
                TestHelpers.Settle(window);

                viewer.IsPaused.Should().BeFalse("AutoScroll=false never pauses");

                for (var i = 200; i < 250; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                TestHelpers.Settle(window);

                viewer.LineCount.Should().Be(250, "records are still applied, only the scrolling stops");
                scrollViewer.Offset.Y.Should().Be(50);
            }
            finally
            {
                window.Close();
            }
        }
    }
}
