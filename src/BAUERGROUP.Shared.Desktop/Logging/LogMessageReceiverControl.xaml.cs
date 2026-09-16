using BAUERGROUP.Shared.Core.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WpfToggleButton = System.Windows.Controls.Primitives.ToggleButton;
using WpfUserControl = System.Windows.Controls.UserControl;

namespace BAUERGROUP.Shared.Desktop.Logging
{
    /// <summary>
    /// Embeddable live log view for the current process. It registers an in-process BGLogger live sink while
    /// it is loaded and visible and removes it again when it is hidden, unloaded or the dispatcher shuts down
    /// (all idempotent; nothing stays registered afterwards). Records are collected off the UI thread in a
    /// bounded buffer and applied in batches by a background-priority timer, so a logging burst can neither
    /// block a logging thread nor starve the UI.
    /// </summary>
    public partial class LogMessageReceiverControl : WpfUserControl
    {
        private const double UpdateIntervalMilliseconds = 150;

        /// <summary>
        /// Upper bound for <see cref="MaxLines"/>. A virtualized view never needs more, and the property is
        /// writable from XAML and from a binding, where an out-of-range value must be clamped rather than
        /// allocate a multi-gigabyte buffer or throw out of the binding engine.
        /// </summary>
        private const int MaxSupportedLines = 100000;

        private readonly BGLogViewBuffer _buffer;
        private readonly LogRecordCollection _lines;
        private readonly DispatcherTimer _timer;

        private bool _attached;
        private bool _scrollPending;

        /// <summary>Maximum number of lines kept in the view; older lines are dropped. Default 5000.</summary>
        public static readonly DependencyProperty MaxLinesProperty =
            DependencyProperty.Register(nameof(MaxLines), typeof(int), typeof(LogMessageReceiverControl),
                new PropertyMetadata(BGLogViewBuffer.DefaultMaxLines, OnMaxLinesChanged, CoerceMaxLines));

        /// <summary>Capture level of the live sink. Default <c>LogLevel.Trace</c>.</summary>
        public static readonly DependencyProperty MinimumLevelProperty =
            DependencyProperty.Register(nameof(MinimumLevel), typeof(LogLevel), typeof(LogMessageReceiverControl),
                new PropertyMetadata(LogLevel.Trace, OnMinimumLevelChanged, CoerceMinimumLevel));

        /// <summary>Follow the newest line. Default true; bound to the "Follow" toggle in the toolbar.</summary>
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.Register(nameof(AutoScroll), typeof(bool), typeof(LogMessageReceiverControl),
                new PropertyMetadata(true, OnAutoScrollChanged));

        /// <summary>
        /// Creates the control. The live sink is registered on <c>Loaded</c> (or when the control becomes
        /// visible), never in the constructor.
        /// </summary>
        public LogMessageReceiverControl()
        {
            InitializeComponent();

            _buffer = new BGLogViewBuffer(MaxLines, MinimumLevel);
            _lines = new LogRecordCollection();
            PART_List.ItemsSource = _lines;

            _timer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher)
            {
                Interval = TimeSpan.FromMilliseconds(UpdateIntervalMilliseconds)
            };
            _timer.Tick += OnTick;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            UpdateStatus();
        }

        /// <summary>
        /// Maximum number of lines kept in the view. Values outside 1 to 100000 are coerced into that range.
        /// Lowering it trims the view on the next update; those trimmed lines are not counted as dropped -
        /// only records that were still waiting for the next update and no longer fit are.
        /// </summary>
        public int MaxLines
        {
            get { return (int)GetValue(MaxLinesProperty); }
            set { SetValue(MaxLinesProperty, value); }
        }

        /// <summary>
        /// Capture level of the live sink (default <c>LogLevel.Trace</c>). Raising it stops the process from
        /// formatting lower-level events while this view is attached; records already shown are kept, and
        /// events below the level were never captured, so lowering it again does not recover history.
        /// A null value is coerced to <c>LogLevel.Trace</c>.
        /// </summary>
        public LogLevel MinimumLevel
        {
            get { return (LogLevel)GetValue(MinimumLevelProperty); }
            set { SetValue(MinimumLevelProperty, value); }
        }

        /// <summary>Follow the newest line. Bound to the "Follow" toggle in the toolbar.</summary>
        public bool AutoScroll
        {
            get { return (bool)GetValue(AutoScrollProperty); }
            set { SetValue(AutoScrollProperty, value); }
        }

        private static object CoerceMaxLines(DependencyObject target, object baseValue)
        {
            var value = (int)baseValue;

            if (value < 1)
                return 1;

            return value > MaxSupportedLines ? MaxSupportedLines : value;
        }

        private static object CoerceMinimumLevel(DependencyObject target, object baseValue)
        {
            return baseValue ?? LogLevel.Trace;
        }

        private static void OnMaxLinesChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ((LogMessageReceiverControl)target)._buffer.MaxLines = (int)e.NewValue;
        }

        private static void OnMinimumLevelChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ((LogMessageReceiverControl)target)._buffer.MinimumLevel = (LogLevel)e.NewValue;
        }

        private static void OnAutoScrollChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ((LogMessageReceiverControl)target).OnAutoScrollChanged((bool)e.NewValue);
        }

        private void OnAutoScrollChanged(bool autoScroll)
        {
            if (PART_Follow != null && PART_Follow.IsChecked != autoScroll)
                PART_Follow.IsChecked = autoScroll;

            // Following again jumps to the newest line instead of waiting for the next record.
            if (autoScroll)
                RequestScrollToNewest();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Only a visible view registers a sink. A control that is loaded while collapsed - a diagnostics
            // pane behind a "show log" toggle - would otherwise capture for the whole process life: false to
            // false is not a change, so UserControl_IsVisibleChanged never fires to switch it off again.
            if (IsVisible)
                Attach();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Detach();
        }

        private void OnDispatcherShutdownStarted(object? sender, EventArgs e)
        {
            Detach();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var visible = (bool)e.NewValue;

            if (visible)
                Attach();
            else
                Detach();
        }

        /// <summary>Registers the live sink and starts the update timer. Idempotent.</summary>
        private void Attach()
        {
            if (_attached)
                return;

            _attached = true;

            // Unhooked again in Detach, so a closed view does not stay alive through the dispatcher.
            Dispatcher.ShutdownStarted += OnDispatcherShutdownStarted;
            _buffer.Attach();
            _timer.Start();
        }

        /// <summary>Stops the update timer and removes the live sink registration. Idempotent.</summary>
        private void Detach()
        {
            if (!_attached)
                return;

            _attached = false;

            Dispatcher.ShutdownStarted -= OnDispatcherShutdownStarted;
            _timer.Stop();
            _buffer.Detach();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            var update = _buffer.Drain();

            if (update.IsEmpty)
            {
                UpdateStatus();
                return;
            }

            _lines.Apply(update);
            UpdateStatus();
            RequestScrollToNewest();
        }

        private void RequestScrollToNewest()
        {
            if (!AutoScroll || _scrollPending || _lines.Count == 0)
                return;

            // Posted at background priority: scrolling in the same step as the batch update is ignored by the
            // virtualizing panel, which has not measured the new items yet.
            _scrollPending = true;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(ScrollToNewest));
        }

        private void ScrollToNewest()
        {
            _scrollPending = false;

            if (_lines.Count == 0)
                return;

            PART_List.ScrollIntoView(_lines[_lines.Count - 1]);
        }

        private void UpdateStatus()
        {
            var dropped = _buffer.DroppedCount;

            PART_Status.Text = dropped > 0
                ? string.Format("{0} lines ({1} dropped)", _lines.Count, dropped)
                : string.Format("{0} lines", _lines.Count);
        }

        private void OnFollowChanged(object sender, RoutedEventArgs e)
        {
            // The toggle is read from the sender: the handler already runs while the XAML is parsed, before
            // the generated PART_Follow field is assigned.
            var toggle = sender as WpfToggleButton;

            if (toggle == null)
                return;

            // SetCurrentValue, not the CLR setter: writing a local value would drop a binding a consumer put
            // on AutoScroll, and this handler also runs for the toggle state that such a binding just pushed.
            SetCurrentValue(AutoScrollProperty, toggle.IsChecked == true);
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            _buffer.Clear();
            _lines.Apply(_buffer.Drain());
            UpdateStatus();
        }

        private void OnCopyAllClick(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(false);
        }

        private void OnCopyCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = PART_List != null && PART_List.SelectedItems.Count > 0;
        }

        private void OnCopyExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            CopyToClipboard(true);
        }

        private void CopyToClipboard(bool selectedOnly)
        {
            var text = GetText(selectedOnly);

            if (text.Length == 0)
                return;

            try
            {
                System.Windows.Clipboard.SetText(text);
            }
            catch (System.Runtime.InteropServices.ExternalException ex)
            {
                // Another process owns the clipboard. Logging is safe here: this runs on the UI thread and
                // never inside a live sink callback.
                BGLogger.Warn(ex, "LogMessageReceiverControl: copying to the clipboard failed.");
            }
        }

        private string GetText(bool selectedOnly)
        {
            IEnumerable<BGLogRecord> records = _lines;

            if (selectedOnly)
            {
                var selected = new HashSet<BGLogRecord>(PART_List.SelectedItems.OfType<BGLogRecord>());

                if (selected.Count == 0)
                    return string.Empty;

                // Kept in list order, not in selection order.
                records = _lines.Where(selected.Contains);
            }

            var builder = new StringBuilder();

            foreach (var record in records)
            {
                if (builder.Length > 0)
                    builder.Append(Environment.NewLine);

                builder.Append(record.DisplayText);
            }

            return builder.ToString();
        }
    }
}
