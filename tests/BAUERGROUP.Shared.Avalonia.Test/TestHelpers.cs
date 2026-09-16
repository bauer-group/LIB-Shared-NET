using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using BAUERGROUP.Shared.Avalonia.Logging;
using BAUERGROUP.Shared.Core.Logging;
using NLog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace BAUERGROUP.Shared.Avalonia.Test
{
    /// <summary>Shared plumbing for the headless log-viewer tests.</summary>
    internal static class TestHelpers
    {
        /// <summary>
        /// Creates a viewer that captures nothing from BGLogger. Records enter the view only through
        /// <see cref="LogViewer.Post"/>, which makes line counts deterministic even though BGLogger is
        /// process-global and other code may log at any time.
        /// </summary>
        internal static LogViewer IsolatedViewer(Int32 maxLines = LogViewer.DefaultMaxLines)
        {
            return new LogViewer { MinimumLevel = LogLevel.Off, MaxLines = maxLines };
        }

        /// <summary>Shows the control in a window and lets the first layout pass run.</summary>
        internal static Window Show(Control content, Double width = 800, Double height = 400)
        {
            var window = new Window { Width = width, Height = height, Content = content };
            window.Show();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            return window;
        }

        /// <summary>
        /// Lets real time pass and runs the dispatcher, so a due <c>DispatcherTimer</c> actually ticks.
        /// <c>Dispatcher.UIThread.RunJobs()</c> alone does not promote a due timer while the job queue is
        /// empty, hence the posted no-op before every pass.
        /// <para>A <c>DispatcherFrame</c> plus <c>DispatcherTimer.RunOnce</c> is deliberately not used: the
        /// frame's exit callback runs at Normal priority and can win the race against the viewer's
        /// Background-priority tick on a loaded machine, which made the wait flaky.</para>
        /// </summary>
        internal static void Pump(Int32 milliseconds)
        {
            var stopwatch = Stopwatch.StartNew();

            do
            {
                PumpOnce();
            }
            while (stopwatch.ElapsedMilliseconds < milliseconds);
        }

        /// <summary>Pumps the dispatcher until the condition holds. False on timeout.</summary>
        internal static Boolean PumpUntil(Func<Boolean> condition, Int32 timeoutMilliseconds = 5000)
        {
            var stopwatch = Stopwatch.StartNew();

            while (!condition())
            {
                if (stopwatch.ElapsedMilliseconds > timeoutMilliseconds)
                    return false;

                PumpOnce();
            }

            return true;
        }

        private static void PumpOnce()
        {
            Thread.Sleep(20);
            Dispatcher.UIThread.Post(() => { });
            Dispatcher.UIThread.RunJobs();
        }

        /// <summary>
        /// Drives layout, queued jobs and the render timer until everything the viewer posted - the deferred
        /// ScrollIntoView above all - has run.
        /// </summary>
        internal static void Settle(Window window)
        {
            for (var i = 0; i < 5; i++)
            {
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            }

            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        /// <summary>
        /// Runs the action on a real worker thread and returns what it threw, or null. A dedicated thread,
        /// not <c>Task.Run(...).Wait()</c>: the waiting UI thread inlines a task that has not started yet,
        /// and the action would then run on the UI thread after all.
        /// </summary>
        internal static Exception? RunOnWorkerThread(Action action)
        {
            Exception? captured = null;

            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    captured = ex;
                }
            });

            thread.IsBackground = true;
            thread.Start();
            thread.Join();

            return captured;
        }

        /// <summary>Creates a record as a live sink would.</summary>
        internal static BGLogRecord Record(LogLevel level, String text)
        {
            return new BGLogRecord(DateTime.Now, level, "BAUERGROUP.Shared.Avalonia.Test", text);
        }

        /// <summary>Creates a record with multi-line exception text, so the row is taller than one line.</summary>
        internal static BGLogRecord RecordWithException(LogLevel level, String text)
        {
            return new BGLogRecord(DateTime.Now, level, "BAUERGROUP.Shared.Avalonia.Test", text,
                                   "System.InvalidOperationException: boom" + Environment.NewLine +
                                   "   at Some.Method()" + Environment.NewLine +
                                   "   at Some.Other.Method()");
        }

        /// <summary>The viewer's list, found through the visual tree (the tests use the public API only).</summary>
        internal static ListBox ListOf(LogViewer viewer)
        {
            return viewer.GetVisualDescendants().OfType<ListBox>().First();
        }

        /// <summary>The list's scroll viewer.</summary>
        internal static ScrollViewer ScrollViewerOf(ListBox list)
        {
            return list.GetVisualDescendants().OfType<ScrollViewer>().First();
        }

        /// <summary>Distance between the current offset and the end of the content.</summary>
        internal static Double DistanceToBottom(ScrollViewer scrollViewer)
        {
            return scrollViewer.Extent.Height - scrollViewer.Viewport.Height - scrollViewer.Offset.Y;
        }
    }
}
