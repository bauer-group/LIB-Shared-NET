using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using BAUERGROUP.Shared.Core.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAUERGROUP.Shared.Avalonia.Logging
{
    /// <summary>
    /// Embeddable live log viewer. Owns its log source: it registers a BGLogger live sink while it is in a
    /// visual tree and its <c>IsVisible</c> is true, and removes it as soon as it is hidden or detached
    /// (both idempotent; nothing stays registered afterwards). A diagnostics pane bound to a "show log"
    /// toggle therefore costs nothing while it is closed. Shows at most <see cref="MaxLines"/> lines in a
    /// virtualized list, follows the newest line, pauses while the user has scrolled up, and copies the
    /// selection with Ctrl+C.
    /// Uses the host application's theme (e.g. FluentTheme) and never registers anything in the XAML designer.
    /// <para>Every member except <see cref="Post"/> must be used from the UI thread.</para>
    /// <para>Note for code inside a <c>BAUERGROUP.Shared.*</c> namespace: this assembly's namespace shadows
    /// the root <c>Avalonia</c> namespace there, so qualify Avalonia types as <c>global::Avalonia.X</c> or
    /// reach them through using-directives.</para>
    /// </summary>
    public partial class LogViewer : UserControl
    {
        /// <summary>Default value of <see cref="MaxLines"/>.</summary>
        public const Int32 DefaultMaxLines = BGLogViewBuffer.DefaultMaxLines;

        /// <summary>
        /// Upper bound for <see cref="MaxLines"/>. A virtualized view never needs more, and the property is
        /// writable from XAML and from a binding, where an out-of-range value must be clamped rather than
        /// allocate a huge buffer or throw out of the binding engine.
        /// </summary>
        private const Int32 MaxSupportedLines = 100000;

        private const Double UpdateIntervalMilliseconds = 150;

        /// <summary>
        /// Distance in device-independent pixels that still counts as "at the newest line". Anything larger is
        /// treated as the user having scrolled up.
        /// </summary>
        private const Double BottomTolerance = 2;

        /// <summary>
        /// Maximum number of lines kept in the view; older lines are dropped. Default 5000, coerced into
        /// 1 to 100000 so the property always reads back the bound the view actually honours.
        /// </summary>
        public static readonly StyledProperty<Int32> MaxLinesProperty =
            AvaloniaProperty.Register<LogViewer, Int32>(nameof(MaxLines), DefaultMaxLines,
                                                        coerce: static (_, value) => ClampMaxLines(value));

        /// <summary>
        /// Capture level of the live sink. Default <c>LogLevel.Trace</c>; null is coerced to
        /// <c>LogLevel.Trace</c>, so the non-nullable property never reads back null.
        /// </summary>
        public static readonly StyledProperty<LogLevel> MinimumLevelProperty =
            AvaloniaProperty.Register<LogViewer, LogLevel>(nameof(MinimumLevel), LogLevel.Trace,
                                                           coerce: static (_, value) => value ?? LogLevel.Trace);

        /// <summary>Follow the newest line. Default true.</summary>
        public static readonly StyledProperty<Boolean> AutoScrollProperty =
            AvaloniaProperty.Register<LogViewer, Boolean>(nameof(AutoScroll), true);

        /// <summary>Show the status/Copy/Clear toolbar. Default true.</summary>
        public static readonly StyledProperty<Boolean> ShowToolbarProperty =
            AvaloniaProperty.Register<LogViewer, Boolean>(nameof(ShowToolbar), true);

        /// <summary>Number of lines currently shown.</summary>
        public static readonly DirectProperty<LogViewer, Int32> LineCountProperty =
            AvaloniaProperty.RegisterDirect<LogViewer, Int32>(nameof(LineCount), o => o.LineCount);

        /// <summary>Number of records waiting for the next update.</summary>
        public static readonly DirectProperty<LogViewer, Int32> PendingCountProperty =
            AvaloniaProperty.RegisterDirect<LogViewer, Int32>(nameof(PendingCount), o => o.PendingCount);

        /// <summary>Number of records discarded before they reached the view.</summary>
        public static readonly DirectProperty<LogViewer, Int64> DroppedCountProperty =
            AvaloniaProperty.RegisterDirect<LogViewer, Int64>(nameof(DroppedCount), o => o.DroppedCount);

        /// <summary>True while the view is frozen because the user scrolled up.</summary>
        public static readonly DirectProperty<LogViewer, Boolean> IsPausedProperty =
            AvaloniaProperty.RegisterDirect<LogViewer, Boolean>(nameof(IsPaused), o => o.IsPaused);

        private readonly AvaloniaList<BGLogRecord> _lines = new AvaloniaList<BGLogRecord>();
        private readonly BGLogViewBuffer _buffer;

        private DispatcherTimer? _timer;
        private ScrollViewer? _scrollViewer;
        private Boolean _initialized;
        private Boolean _inVisualTree;
        private Boolean _capturing;
        private Boolean _scrollPending;
        private Boolean _suppressScroll;

        private Int32 _lineCount;
        private Int32 _pendingCount;
        private Int64 _droppedCount;
        private Boolean _isPaused;

        /// <summary>
        /// Creates the viewer. The live sink is registered once the control is visible in a visual tree,
        /// never in the constructor.
        /// </summary>
        public LogViewer()
        {
            // Created before InitializeComponent so that property values assigned by the XAML parser (and by
            // styles) already find a buffer to forward to. Both property values are coerced into range.
            _buffer = new BGLogViewBuffer(MaxLines, MinimumLevel);

            InitializeComponent();

            PART_List.ItemsSource = _lines;
            PART_List.TemplateApplied += OnListTemplateApplied;

            PART_Copy.Click += (_, _) => Copy(true);
            PART_Clear.Click += (_, _) => Clear();
            PART_Resume.Click += (_, _) => ScrollToNewest();
            PART_MenuCopy.Click += (_, _) => Copy(true);
            PART_MenuCopyAll.Click += (_, _) => Copy(false);
            PART_MenuClear.Click += (_, _) => Clear();

            _initialized = true;
            PART_Toolbar.IsVisible = ShowToolbar;
            UpdateStatus();
        }

        /// <summary>
        /// Maximum number of lines kept in the view. Values outside 1 to 100000 are coerced into that range,
        /// so the getter reports the bound that is actually in force. Lowering it trims the view on the next
        /// update.
        /// </summary>
        public Int32 MaxLines
        {
            get { return GetValue(MaxLinesProperty); }
            set { SetValue(MaxLinesProperty, value); }
        }

        /// <summary>
        /// Capture level (default Trace). Raising it stops the process from formatting lower-level events
        /// while this viewer is open; records already shown are kept, and events below the level were never
        /// captured, so lowering it again does not recover history. A null value is coerced to
        /// <c>LogLevel.Trace</c>.
        /// </summary>
        public LogLevel MinimumLevel
        {
            get { return GetValue(MinimumLevelProperty); }
            set { SetValue(MinimumLevelProperty, value); }
        }

        /// <summary>Follow the newest line.</summary>
        public Boolean AutoScroll
        {
            get { return GetValue(AutoScrollProperty); }
            set { SetValue(AutoScrollProperty, value); }
        }

        /// <summary>Show the status/Copy/Clear toolbar.</summary>
        public Boolean ShowToolbar
        {
            get { return GetValue(ShowToolbarProperty); }
            set { SetValue(ShowToolbarProperty, value); }
        }

        /// <summary>Number of lines currently shown.</summary>
        public Int32 LineCount
        {
            get { return _lineCount; }
            private set { SetAndRaise(LineCountProperty, ref _lineCount, value); }
        }

        /// <summary>Number of records waiting for the next update.</summary>
        public Int32 PendingCount
        {
            get { return _pendingCount; }
            private set { SetAndRaise(PendingCountProperty, ref _pendingCount, value); }
        }

        /// <summary>
        /// Records discarded before they reached the view, because the bounded pending buffer overflowed or
        /// because <see cref="MaxLines"/> was lowered. Never reset by <see cref="Clear"/>.
        /// </summary>
        public Int64 DroppedCount
        {
            get { return _droppedCount; }
            private set { SetAndRaise(DroppedCountProperty, ref _droppedCount, value); }
        }

        /// <summary>
        /// True while the view is frozen because the user scrolled up. New records keep arriving in the
        /// bounded pending buffer; the oldest are dropped and counted in <see cref="DroppedCount"/>.
        /// </summary>
        public Boolean IsPaused
        {
            get { return _isPaused; }
            private set { SetAndRaise(IsPausedProperty, ref _isPaused, value); }
        }

        /// <summary>
        /// Appends a record from any thread without going through BGLogger (tests, custom sources). The record
        /// becomes visible with the next update.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="record"/> is null.</exception>
        public void Post(BGLogRecord record)
        {
            _buffer.Post(record);
        }

        /// <summary>
        /// Applies pending records to the view immediately. Does nothing while <see cref="IsPaused"/>.
        /// Must be called from the UI thread.
        /// </summary>
        public void Refresh()
        {
            if (IsPaused)
            {
                UpdateCounters();
                return;
            }

            Apply(_buffer.Drain());
        }

        /// <summary>
        /// Empties the view and discards everything still pending. Resumes following, so a paused and now
        /// empty view is not stuck. Must be called from the UI thread.
        /// </summary>
        public void Clear()
        {
            _buffer.Clear();
            SetPaused(false);
            Apply(_buffer.Drain());
        }

        /// <summary>
        /// Resumes following, applies pending records and scrolls to the newest line. Scrolls even while
        /// <see cref="AutoScroll"/> is false: this is an explicit jump-to-end command, not the automatic
        /// follow path. Must be called from the UI thread.
        /// </summary>
        public void ScrollToNewest()
        {
            SetPaused(false);
            Apply(_buffer.Drain());

            // Also requested when the update was empty: "jump to newest" must work on an idle view.
            RequestScrollToNewest(true);
        }

        /// <summary>
        /// The selected lines (or all lines) joined with <see cref="Environment.NewLine"/>, in list order -
        /// not in selection order. Must be called from the UI thread.
        /// </summary>
        /// <param name="selectedOnly">True to return only the selected lines.</param>
        /// <exception cref="InvalidOperationException">Called from a thread other than the UI thread.</exception>
        public String GetText(Boolean selectedOnly = false)
        {
            // The view is mutated by the update timer on the UI thread; enumerating it from a worker would
            // either throw mid-enumeration or return a torn snapshot. Fail like every other member instead.
            Dispatcher.UIThread.VerifyAccess();

            IEnumerable<BGLogRecord> records = _lines;

            if (selectedOnly)
            {
                var selected = PART_List.SelectedItems;

                if (selected == null || selected.Count == 0)
                    return String.Empty;

                var set = new HashSet<BGLogRecord>(selected.OfType<BGLogRecord>());

                if (set.Count == 0)
                    return String.Empty;

                records = _lines.Where(set.Contains);
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

        /// <summary>
        /// Copies <see cref="GetText(Boolean)"/> to the clipboard. False when there is no clipboard (the
        /// control is not in a window) or nothing to copy. Must be called from the UI thread.
        /// </summary>
        /// <param name="selectedOnly">True to copy only the selected lines.</param>
        public async Task<Boolean> CopyToClipboardAsync(Boolean selectedOnly = true)
        {
            try
            {
                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

                if (clipboard == null)
                    return false;

                var text = GetText(selectedOnly);

                if (text.Length == 0)
                    return false;

                await clipboard.SetTextAsync(text).ConfigureAwait(true);
                return true;
            }
            catch (Exception ex)
            {
                // Runs on the UI thread and never inside a live-sink callback, so logging is safe here.
                BGLogger.Warn(ex, "LogViewer: copying to the clipboard failed.");
                return false;
            }
        }

        /// <summary>
        /// Registers the live sink and starts the update timer, unless the control is hidden. Idempotent.
        /// </summary>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            _inVisualTree = true;
            PART_Toolbar.IsVisible = ShowToolbar;
            UpdateCaptureState();
        }

        /// <summary>
        /// Stops the update timer and removes the live sink registration. Idempotent. The lines already shown
        /// are kept, so re-attaching resumes the same view.
        /// </summary>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            _inVisualTree = false;
            UpdateCaptureState();
        }

        /// <summary>
        /// Registers or removes the live sink so that only a viewer that is in a visual tree and not hidden
        /// captures. Idempotent in both directions.
        /// <para>The control's own <c>IsVisible</c> is the criterion, not <c>IsEffectivelyVisible</c>:
        /// Avalonia 12.1.2 raises no public notification when an ancestor's visibility flips, so a viewer
        /// gated on the effective value could never learn that it became visible again and would stay dark
        /// for good.</para>
        /// </summary>
        private void UpdateCaptureState()
        {
            var capture = _inVisualTree && IsVisible;

            if (capture == _capturing)
                return;

            _capturing = capture;

            if (capture)
            {
                // The XAML previewer must never construct BGLogger or touch the process logging configuration.
                if (!Design.IsDesignMode)
                    _buffer.Attach();

                _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(UpdateIntervalMilliseconds),
                                             DispatcherPriority.Background, (_, _) => OnTick());
                _timer.Start();
                return;
            }

            // Timer first, then the sink: a tick must never run against a detached buffer.
            _timer?.Stop();
            _timer = null;
            _buffer.Detach();

            // Detach discards the pending records, so the counters would otherwise keep reporting the
            // pre-detach numbers until the first tick after a re-attach.
            UpdateCounters();
        }

        /// <summary>Ctrl+C (Cmd+C) copies the selection, End jumps to the newest line.</summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Handled)
                return;

            if (e.Key == Key.C &&
                ((e.KeyModifiers & KeyModifiers.Control) != 0 || (e.KeyModifiers & KeyModifiers.Meta) != 0))
            {
                e.Handled = true;
                Copy(true);
                return;
            }

            if (e.Key == Key.End)
            {
                e.Handled = true;
                ScrollToNewest();
            }
        }

        /// <inheritdoc />
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            // Styles and the XAML parser can assign before the constructor body has run.
            if (_buffer == null)
                return;

            // Both values arrive coerced into range by the property registrations.
            if (change.Property == MaxLinesProperty)
            {
                _buffer.MaxLines = change.GetNewValue<Int32>();
                return;
            }

            if (change.Property == MinimumLevelProperty)
            {
                _buffer.MinimumLevel = change.GetNewValue<LogLevel>();
                return;
            }

            // A viewer hidden behind a "show log" toggle must stop capturing, and start again when shown.
            if (change.Property == IsVisibleProperty)
            {
                UpdateCaptureState();
                return;
            }

            // The named parts only exist once the constructor has run InitializeComponent; a value assigned
            // by the XAML parser or by a style before that is re-applied in OnAttachedToVisualTree.
            if (!_initialized)
                return;

            if (change.Property == ShowToolbarProperty)
            {
                PART_Toolbar.IsVisible = change.GetNewValue<Boolean>();
            }
            else if (change.Property == AutoScrollProperty)
            {
                if (change.GetNewValue<Boolean>())
                    ScrollToNewest();
                else
                    SetPaused(false);   // a view that does not follow is never "paused"
            }
        }

        /// <summary>
        /// Loads the XAML. Hand-written instead of generated (the project sets
        /// <c>AvaloniaNameGeneratorBehavior=OnlyProperties</c>): the generated one is public, and a consumer
        /// calling it a second time would reload the XAML and silently drop the ItemsSource and the click
        /// handlers the constructor wires once.
        /// </summary>
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private static Int32 ClampMaxLines(Int32 value)
        {
            if (value < 1)
                return 1;

            return value > MaxSupportedLines ? MaxSupportedLines : value;
        }

        private void OnListTemplateApplied(Object? sender, TemplateAppliedEventArgs e)
        {
            if (_scrollViewer != null)
                _scrollViewer.ScrollChanged -= OnScrollChanged;

            _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");

            if (_scrollViewer != null)
                _scrollViewer.ScrollChanged += OnScrollChanged;
        }

        private void OnTick()
        {
            if (IsPaused)
            {
                UpdateCounters();
                return;
            }

            Apply(_buffer.Drain());
        }

        private void Apply(BGLogViewUpdate update)
        {
            if (!update.IsEmpty)
            {
                // Every list mutation below moves the scroll offset; the pause detector must not read those
                // movements as a user scrolling up.
                _suppressScroll = true;

                if (update.IsReset)
                    _lines.Clear();
                else if (update.RemoveFromStart > 0)
                    _lines.RemoveRange(0, Math.Min(update.RemoveFromStart, _lines.Count));

                if (update.Added.Count > 0)
                    _lines.AddRange(update.Added);
            }

            UpdateCounters();

            if (update.IsEmpty)
                return;

            if (AutoScroll && !IsPaused)
                RequestScrollToNewest();
            else
                _suppressScroll = false;
        }

        /// <param name="force">
        /// True for an explicit jump-to-end command, which scrolls even while <see cref="AutoScroll"/> is
        /// false. The automatic follow path never forces.
        /// </param>
        private void RequestScrollToNewest(Boolean force = false)
        {
            if ((!AutoScroll && !force) || IsPaused)
            {
                _suppressScroll = false;
                return;
            }

            if (_lines.Count == 0)
            {
                _suppressScroll = false;
                return;
            }

            if (_scrollPending)
                return;

            _scrollPending = true;
            _suppressScroll = true;
            Dispatcher.UIThread.Post(ScrollCore, DispatcherPriority.Background);
        }

        private void ScrollCore()
        {
            _scrollPending = false;

            // Posted, never called in the same step as the list update: the virtualizing panel has not
            // measured the new items yet and silently ignores a ScrollIntoView for them.
            if (_lines.Count > 0)
                PART_List.ScrollIntoView(_lines.Count - 1);

            // Released only after the scroll-driven layout pass has raised its ScrollChanged.
            Dispatcher.UIThread.Post(
                () =>
                {
                    if (!_scrollPending)
                        _suppressScroll = false;
                },
                DispatcherPriority.Background);
        }

        private void OnScrollChanged(Object? sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = _scrollViewer;

            if (scrollViewer == null || !AutoScroll)
                return;

            // Layout-driven, not the user: adding, removing or resizing rows changes the extent or the
            // viewport and moves the offset with it.
            if (e.ExtentDelta.Y != 0 || e.ViewportDelta.Y != 0)
                return;

            var distance = scrollViewer.Extent.Height - scrollViewer.Viewport.Height - scrollViewer.Offset.Y;

            if (!IsPaused)
            {
                if (!_suppressScroll && e.OffsetDelta.Y < 0 && distance > BottomTolerance)
                    SetPaused(true);
            }
            else if (distance <= BottomTolerance)
            {
                SetPaused(false);
            }
        }

        private void SetPaused(Boolean paused)
        {
            if (IsPaused == paused)
                return;

            IsPaused = paused;
            PART_Resume.IsVisible = paused;
            UpdateStatus();
        }

        private void UpdateCounters()
        {
            LineCount = _lines.Count;
            PendingCount = _buffer.PendingCount;
            DroppedCount = _buffer.DroppedCount;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            PART_Status.Text = DroppedCount > 0
                ? String.Format("{0} lines ({1} dropped)", LineCount, DroppedCount)
                : String.Format("{0} lines", LineCount);

            PART_Resume.Content = String.Format("Paused · {0} new — jump to newest", PendingCount);
        }

        private void Copy(Boolean selectedOnly)
        {
            // Fire and forget: the task reports failures through BGLogger itself.
            _ = CopyToClipboardAsync(selectedOnly);
        }
    }
}
