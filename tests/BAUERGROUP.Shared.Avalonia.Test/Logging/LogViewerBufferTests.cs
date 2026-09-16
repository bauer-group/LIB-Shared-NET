using Avalonia.Headless.XUnit;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace BAUERGROUP.Shared.Avalonia.Test.Logging
{
    /// <summary>
    /// Buffering behaviour: nothing reaches the list outside the update timer, updates are applied as whole
    /// batches, and the view stays bounded by MaxLines.
    /// </summary>
    public class LogViewerBufferTests
    {
        [AvaloniaFact]
        public void BackgroundThreadPost_ShouldNotTouchLinesUntilTimerDrains()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                TestHelpers.RunOnWorkerThread(() =>
                {
                    for (var i = 0; i < 10; i++)
                        viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));
                }).Should().BeNull();

                viewer.LineCount.Should().Be(0, "a producer thread never touches the list");
                viewer.PendingCount.Should().Be(0, "the counter is only refreshed by the update timer");

                // No Refresh(): only the control's own DispatcherTimer may move the records into the list.
                TestHelpers.PumpUntil(() => viewer.LineCount == 10)
                           .Should().BeTrue("the update timer drains the buffer on its own");

                viewer.PendingCount.Should().Be(0);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Drain_ShouldRaiseExactlyOneAddEvent()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                var list = TestHelpers.ListOf(viewer);
                var events = new List<NotifyCollectionChangedEventArgs>();
                ((INotifyCollectionChanged)list.ItemsSource!).CollectionChanged += (_, e) => events.Add(e);

                for (var i = 0; i < 20; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();

                events.Should().ContainSingle();
                events[0].Action.Should().Be(NotifyCollectionChangedAction.Add);
                events[0].NewItems!.Count.Should().Be(20);
                viewer.LineCount.Should().Be(20);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void OverflowDrain_ShouldRaiseOneRemoveThenOneAdd()
        {
            var viewer = TestHelpers.IsolatedViewer(50);
            var window = TestHelpers.Show(viewer);

            try
            {
                var list = TestHelpers.ListOf(viewer);

                for (var i = 0; i < 50; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                viewer.LineCount.Should().Be(50);

                var events = new List<NotifyCollectionChangedEventArgs>();
                ((INotifyCollectionChanged)list.ItemsSource!).CollectionChanged += (_, e) => events.Add(e);

                for (var i = 50; i < 80; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();

                events.Should().HaveCount(2);
                events[0].Action.Should().Be(NotifyCollectionChangedAction.Remove);
                events[0].OldItems!.Count.Should().Be(30);
                events[1].Action.Should().Be(NotifyCollectionChangedAction.Add);
                events[1].NewItems!.Count.Should().Be(30);
                viewer.LineCount.Should().Be(50);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void MaxLines_ShouldKeepNewestAndDropOldest()
        {
            var viewer = TestHelpers.IsolatedViewer(10);
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 25; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();

                viewer.LineCount.Should().Be(10);

                var text = viewer.GetText();
                text.Should().Contain("line 24").And.Contain("line 15");
                text.Should().NotContain("line 14").And.NotContain("line 9");
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void MaxLines_Decrease_ShouldTrimImmediatelyOnNextRefresh()
        {
            var viewer = TestHelpers.IsolatedViewer(100);
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 50; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                viewer.LineCount.Should().Be(50);

                viewer.MaxLines = 10;
                viewer.Refresh();

                viewer.LineCount.Should().Be(10, "a lowered bound trims the view without any pending record");
                viewer.GetText().Should().Contain("line 49").And.NotContain("line 39");
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Burst_LargerThanMaxLines_ShouldReportDroppedCount()
        {
            var viewer = TestHelpers.IsolatedViewer(10);
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 25; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();

                viewer.LineCount.Should().Be(10);
                viewer.DroppedCount.Should().Be(15);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Clear_ShouldEmptyTheView()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 10; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                viewer.LineCount.Should().Be(10);

                viewer.Clear();

                viewer.LineCount.Should().Be(0);
                viewer.PendingCount.Should().Be(0);
                viewer.GetText().Should().BeEmpty();

                // The view keeps working afterwards.
                viewer.Post(TestHelpers.Record(LogLevel.Info, "after clear"));
                viewer.Refresh();
                viewer.LineCount.Should().Be(1);
            }
            finally
            {
                window.Close();
            }
        }
    }
}
