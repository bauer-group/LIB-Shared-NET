using Avalonia.Headless.XUnit;
using BAUERGROUP.Shared.Avalonia.Logging;
using NLog;
using System;
using System.Reflection;

namespace BAUERGROUP.Shared.Avalonia.Test.Logging
{
    /// <summary>
    /// The public contract a consumer binds against: properties read back what the view honours, and the
    /// members that touch the view refuse to run off the UI thread.
    /// </summary>
    public class LogViewerApiTests
    {
        [AvaloniaTheory]
        [InlineData(0, 1)]
        [InlineData(-7, 1)]
        [InlineData(1, 1)]
        [InlineData(250, 250)]
        [InlineData(100000, 100000)]
        [InlineData(999999, 100000)]
        public void MaxLines_ShouldReadBackCoerced(Int32 assigned, Int32 expected)
        {
            var viewer = TestHelpers.IsolatedViewer();

            viewer.MaxLines = assigned;

            viewer.MaxLines.Should().Be(expected, "a two-way binding must round-trip the honoured bound");
        }

        [AvaloniaFact]
        public void MaxLines_OutOfRange_ShouldAlsoBindTheView()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                viewer.MaxLines = 0;

                for (var i = 0; i < 20; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();

                viewer.LineCount.Should().Be(1, "the effective bound is the coerced one");
                viewer.MaxLines.Should().Be(1, "and the getter reports it");
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void MinimumLevel_Null_ShouldCoerceToTrace()
        {
            var viewer = new LogViewer();

            viewer.MinimumLevel = null!;

            viewer.MinimumLevel.Should().BeSameAs(LogLevel.Trace,
                "a non-nullable property must never read back null");
        }

        [AvaloniaFact]
        public void InitializeComponent_ShouldNotBePublic()
        {
            // The generated InitializeComponent would reload the XAML and silently drop the ItemsSource and
            // the click handlers the constructor wires, so it must not be part of the package surface.
            typeof(LogViewer).GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance)
                             .Should().BeNull();
        }

        [AvaloniaFact]
        public void GetText_FromAnotherThread_ShouldThrow()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 5; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();

                var captured = TestHelpers.RunOnWorkerThread(() => viewer.GetText());

                captured.Should().BeOfType<InvalidOperationException>(
                    "enumerating the view from a worker would tear while the update timer mutates it");
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Post_FromAnotherThread_ShouldBeAllowed()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                var captured = TestHelpers.RunOnWorkerThread(
                    () => viewer.Post(TestHelpers.Record(LogLevel.Info, "from a worker")));

                captured.Should().BeNull("Post is the one member documented as callable from any thread");

                viewer.Refresh();
                viewer.GetText().Should().Contain("from a worker");
            }
            finally
            {
                window.Close();
            }
        }
    }
}
