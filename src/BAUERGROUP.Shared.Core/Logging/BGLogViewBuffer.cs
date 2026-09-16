using NLog;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BAUERGROUP.Shared.Core.Logging
{
    /// <summary>
    /// UI-agnostic model behind a live log view; shared by the WPF and the Avalonia viewer.
    /// <see cref="Post"/> is thread-safe and may be called from any thread. Every other member must be
    /// called from the thread that owns the view (the UI thread). Pending records are bounded by
    /// <see cref="MaxLines"/>; overflow drops the oldest and increases <see cref="DroppedCount"/>.
    /// </summary>
    public sealed class BGLogViewBuffer : IDisposable
    {
        /// <summary>Default bound for the pending buffer and for the view.</summary>
        public const int DefaultMaxLines = 5000;

        private readonly Func<Action<BGLogRecord>, LogLevel, IDisposable>? _subscribe;
        private readonly BGLogRingBuffer _pending;
        private readonly List<BGLogRecord> _scratch = new List<BGLogRecord>();

        private IDisposable? _subscription;
        private object? _token;
        private int _maxLines;
        private LogLevel _minimumLevel;
        private int _viewCount;
        private bool _resetRequested;

        /// <param name="maxLines">Bound for the pending buffer and for the view; values &lt; 1 throw.</param>
        /// <param name="minimumLevel">Capture level; null means <c>LogLevel.Trace</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLines"/> is less than 1.</exception>
        public BGLogViewBuffer(int maxLines = DefaultMaxLines, LogLevel? minimumLevel = null)
            : this(maxLines, minimumLevel, null)
        {
        }

        /// <param name="maxLines">Bound for the pending buffer and for the view; values &lt; 1 throw.</param>
        /// <param name="minimumLevel">Capture level; null means <c>LogLevel.Trace</c>.</param>
        /// <param name="subscribe">
        /// Registration factory. Null uses <c>BGLogger.Configuration.AddLiveSink</c>. A non-null delegate lets
        /// a consumer (or a test) feed the buffer from another source; it is called on <see cref="Attach"/> and
        /// its result disposed on <see cref="Detach"/>.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLines"/> is less than 1.</exception>
        public BGLogViewBuffer(int maxLines, LogLevel? minimumLevel,
                               Func<Action<BGLogRecord>, LogLevel, IDisposable>? subscribe)
        {
            if (maxLines < 1)
                throw new ArgumentOutOfRangeException(nameof(maxLines), maxLines, "MaxLines must be at least 1.");

            _maxLines = maxLines;
            _minimumLevel = minimumLevel ?? LogLevel.Trace;
            _subscribe = subscribe;
            _pending = new BGLogRingBuffer(maxLines);
        }

        /// <summary>
        /// Bound for pending records and for the view. Setting it resizes the pending buffer
        /// (keeping the newest) and makes the next <see cref="Drain"/> trim the view. Pending records that no
        /// longer fit a lowered bound are discarded and counted in <see cref="DroppedCount"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is less than 1.</exception>
        public int MaxLines
        {
            get { return _maxLines; }

            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "MaxLines must be at least 1.");

                if (value == _maxLines)
                    return;

                _maxLines = value;
                _pending.Resize(value);
            }
        }

        /// <summary>
        /// Capture level. Setting it while attached re-registers the sink: events logged during the
        /// swap can be missed, and events below the level were never captured, so lowering it again does not
        /// recover history. Records already in the view are kept.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value is null.</exception>
        public LogLevel MinimumLevel
        {
            get { return _minimumLevel; }

            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (value.Ordinal == _minimumLevel.Ordinal)
                    return;

                _minimumLevel = value;

                if (_subscription != null)
                {
                    // Keep the pending records: only the registration is swapped, not the view.
                    DetachCore(false);
                    Attach();
                }
            }
        }

        /// <summary>True while a live sink registration is held.</summary>
        public bool IsAttached
        {
            get { return _subscription != null; }
        }

        /// <summary>Number of records waiting for the next <see cref="Drain"/>.</summary>
        public int PendingCount
        {
            get { return _pending.Count; }
        }

        /// <summary>
        /// Records discarded before they reached the view: dropped because the pending buffer was full, or
        /// discarded by a <see cref="MaxLines"/> decrease. Never reset by <see cref="Clear"/>.
        /// </summary>
        public long DroppedCount
        {
            get { return _pending.DroppedCount; }
        }

        /// <summary>Registers the live sink. Idempotent.</summary>
        public void Attach()
        {
            if (_subscription != null)
                return;

            // Per-attach token: a sink that is still in flight while Detach runs cannot enqueue afterwards.
            var token = new object();
            Volatile.Write(ref _token, token);

            var subscribe = _subscribe ?? DefaultSubscribe;
            _subscription = subscribe(
                record =>
                {
                    if (ReferenceEquals(Volatile.Read(ref _token), token))
                        _pending.Add(record);
                },
                _minimumLevel);
        }

        /// <summary>
        /// Removes the live sink registration. Idempotent; pending records are discarded, the view is not
        /// touched.
        /// </summary>
        public void Detach()
        {
            DetachCore(true);
        }

        /// <summary>Appends a record from any thread.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="record"/> is null.</exception>
        public void Post(BGLogRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            _pending.Add(record);
        }

        /// <summary>
        /// Returns the changes the view has to apply. Must be called from the thread that owns the view.
        /// One drain can never return more than <see cref="MaxLines"/> records.
        /// </summary>
        public BGLogViewUpdate Drain()
        {
            _scratch.Clear();
            _pending.DrainTo(_scratch);

            var reset = _resetRequested;
            _resetRequested = false;
            var max = _maxLines;

            if (reset)
            {
                _viewCount = _scratch.Count;
                return new BGLogViewUpdate(true, 0, _scratch.ToArray());
            }

            var removeFromStart = Math.Max(0, _viewCount + _scratch.Count - max);
            if (removeFromStart == 0 && _scratch.Count == 0)
                return default;                 // idle tick: no allocation

            _viewCount = _viewCount - removeFromStart + _scratch.Count;
            return new BGLogViewUpdate(false, removeFromStart, _scratch.ToArray());
        }

        /// <summary>Discards the pending records and makes the next <see cref="Drain"/> request a reset.</summary>
        public void Clear()
        {
            _pending.Clear();
            _resetRequested = true;
            _viewCount = 0;
        }

        /// <summary>Removes the live sink registration. Idempotent.</summary>
        public void Dispose()
        {
            Detach();
        }

        private void DetachCore(bool clearPending)
        {
            Volatile.Write(ref _token, null);

            var subscription = _subscription;
            _subscription = null;

            // The buffer state is reset even when a consumer-supplied registration throws on dispose, so
            // Detach and Dispose stay usable from a window-closing or shutdown handler.
            try
            {
                subscription?.Dispose();
            }
            finally
            {
                if (clearPending)
                    _pending.Clear();
            }
        }

        private static IDisposable DefaultSubscribe(Action<BGLogRecord> sink, LogLevel minimumLevel)
        {
            return BGLogger.Configuration.AddLiveSink(sink, minimumLevel);
        }
    }
}
