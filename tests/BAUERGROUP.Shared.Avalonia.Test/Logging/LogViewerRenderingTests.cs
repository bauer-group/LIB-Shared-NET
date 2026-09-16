using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using BAUERGROUP.Shared.Avalonia.Logging;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BAUERGROUP.Shared.Avalonia.Test.Logging
{
    /// <summary>Virtualization, row density, theme colours and the copy paths.</summary>
    public class LogViewerRenderingTests
    {
        [AvaloniaFact]
        public void FiveThousandLines_ShouldRealizeFewerThanOneHundredContainers()
        {
            var viewer = TestHelpers.IsolatedViewer(5000);
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 5000; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                TestHelpers.Settle(window);

                var list = TestHelpers.ListOf(viewer);

                viewer.LineCount.Should().Be(5000);
                list.ItemsPanelRoot.Should().BeOfType<VirtualizingStackPanel>();

                var realized = list.GetRealizedContainers().Count();
                realized.Should().BeGreaterThan(0).And.BeLessThan(100);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Rows_ShouldBeDense()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 50; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                TestHelpers.Settle(window);

                var list = TestHelpers.ListOf(viewer);
                var height = list.GetRealizedContainers().First().Bounds.Height;

                height.Should().BeGreaterThan(0);
                height.Should().BeLessThanOrEqualTo(22, "the ListBoxItem style overrides the Fluent default height");
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void WarningAndErrorRows_ShouldUseThemeBrushes_InDefaultAndDark()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                viewer.Post(TestHelpers.Record(LogLevel.Warn, "a warning"));
                viewer.Post(TestHelpers.Record(LogLevel.Error, "an error"));
                viewer.Refresh();
                TestHelpers.Settle(window);

                viewer.TryFindResource("LogViewerWarningForeground", ThemeVariant.Default, out var defaultWarning)
                      .Should().BeTrue();
                viewer.TryFindResource("LogViewerErrorForeground", ThemeVariant.Default, out var defaultError)
                      .Should().BeTrue();
                viewer.TryFindResource("LogViewerErrorForeground", ThemeVariant.Dark, out var darkError)
                      .Should().BeTrue();

                ((ISolidColorBrush)defaultWarning!).Color.Should().Be(Color.Parse("#9D5D00"));
                ((ISolidColorBrush)defaultError!).Color.Should().Be(Color.Parse("#C42B1C"));
                ((ISolidColorBrush)darkError!).Color.Should().Be(Color.Parse("#FF99A4"));

                ForegroundOf(viewer, "warn").Should().Be(Color.Parse("#9D5D00"));
                ForegroundOf(viewer, "error").Should().Be(Color.Parse("#C42B1C"));

                window.RequestedThemeVariant = ThemeVariant.Dark;
                TestHelpers.Settle(window);

                ForegroundOf(viewer, "warn").Should().Be(Color.Parse("#FCE100"));
                ForegroundOf(viewer, "error").Should().Be(Color.Parse("#FF99A4"));

                window.RequestedThemeVariant = ThemeVariant.Light;
                TestHelpers.Settle(window);

                ForegroundOf(viewer, "warn").Should().Be(Color.Parse("#9D5D00"),
                    "\"Default\" is the fallback for the Light variant");
                ForegroundOf(viewer, "error").Should().Be(Color.Parse("#C42B1C"));
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void ViewerResourcesKey_ShouldOverrideTheThemeBrush()
        {
            var viewer = TestHelpers.IsolatedViewer();

            // A direct key in the same dictionary wins over its theme dictionaries.
            viewer.Resources["LogViewerErrorForeground"] = new SolidColorBrush(Color.Parse("#FF00FF"));

            var window = TestHelpers.Show(viewer);

            try
            {
                viewer.Post(TestHelpers.Record(LogLevel.Error, "an error"));
                viewer.Refresh();
                TestHelpers.Settle(window);

                ForegroundOf(viewer, "error").Should().Be(Color.Parse("#FF00FF"));

                window.RequestedThemeVariant = ThemeVariant.Dark;
                TestHelpers.Settle(window);

                ForegroundOf(viewer, "error").Should().Be(Color.Parse("#FF00FF"),
                    "the override applies to every theme variant");
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public async Task CopySelected_ShouldRoundTripThroughTheClipboard()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 3; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();
                TestHelpers.Settle(window);

                var list = TestHelpers.ListOf(viewer);
                list.SelectAll();

                (await viewer.CopyToClipboardAsync(true)).Should().BeTrue();

                var clipboard = TopLevel.GetTopLevel(viewer)!.Clipboard!;
                var text = await clipboard.TryGetTextAsync();

                text.Should().NotBeNull();
                text!.Should().Contain("line 0").And.Contain("line 2");
                text.Should().Be(viewer.GetText());
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void GetText_All_ShouldJoinEveryLineInOrder()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 5; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "line " + i));

                viewer.Refresh();

                var lines = viewer.GetText().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                lines.Should().HaveCount(5);

                for (var i = 0; i < 5; i++)
                    lines[i].Should().EndWith("line " + i);

                viewer.GetText(true).Should().BeEmpty("nothing is selected");
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public async Task CtrlC_ShouldCopyTheSelection()
        {
            var viewer = TestHelpers.IsolatedViewer();
            var window = TestHelpers.Show(viewer);

            try
            {
                for (var i = 0; i < 3; i++)
                    viewer.Post(TestHelpers.Record(LogLevel.Info, "ctrl-c line " + i));

                viewer.Refresh();
                TestHelpers.Settle(window);

                var list = TestHelpers.ListOf(viewer);
                list.SelectAll();

                var args = new KeyEventArgs
                {
                    RoutedEvent = InputElement.KeyDownEvent,
                    Key = Key.C,
                    KeyModifiers = KeyModifiers.Control,
                };

                viewer.RaiseEvent(args);
                args.Handled.Should().BeTrue();

                await Task.Delay(200);

                var clipboard = TopLevel.GetTopLevel(viewer)!.Clipboard!;
                var text = await clipboard.TryGetTextAsync();

                text.Should().NotBeNull();
                text!.Should().Contain("ctrl-c line 0").And.Contain("ctrl-c line 2");
            }
            finally
            {
                window.Close();
            }
        }

        private static Color ForegroundOf(LogViewer viewer, String styleClass)
        {
            var list = TestHelpers.ListOf(viewer);

            var textBlock = list.GetVisualDescendants()
                                .OfType<TextBlock>()
                                .First(t => t.Classes.Contains(styleClass));

            return ((ISolidColorBrush)textBlock.Foreground!).Color;
        }
    }
}
