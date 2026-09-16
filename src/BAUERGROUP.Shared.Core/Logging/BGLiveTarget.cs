using NLog;
using NLog.Common;
using NLog.Targets;
using System;
using System.Threading;

namespace BAUERGROUP.Shared.Core.Logging
{
    /// <summary>
    /// NLog target that hands every log event to the in-process live sinks registered through
    /// <see cref="BGLoggerConfiguration.AddLiveSink(Action{BGLogRecord}, LogLevel)"/>.
    /// </summary>
    /// <remarks>
    /// The sink array is swapped copy-on-write; <see cref="AddSink"/> and <see cref="RemoveSink"/> are only
    /// ever called while <see cref="BGLoggerConfiguration"/> holds its live lock, so the swap needs no lock
    /// of its own. The target never calls <c>BGLogger</c> — failures go to NLog's
    /// <see cref="InternalLogger"/> only.
    /// </remarks>
    internal sealed class BGLiveTarget : TargetWithLayout
    {
        [ThreadStatic]
        private static bool t_inWrite;

        private BGLiveSink[] _sinks = Array.Empty<BGLiveSink>();

        internal BGLiveTarget()
        {
            // AddTarget does not assign the name, and the name-based rule stripping inside
            // LoggingConfiguration.RemoveTarget depends on it.
            Name = "LIVE";
            Layout = "${message}";
        }

        /// <summary>
        /// True while the calling thread is inside <see cref="Write"/>, that is, inside a sink callback.
        /// Registering or removing a sink from there would take the configuration's live lock while NLog's
        /// per-target lock is already held, which deadlocks against any other thread doing the reverse.
        /// </summary>
        internal static bool IsInWrite
        {
            get { return t_inWrite; }
        }

        /// <summary>Number of registered sinks.</summary>
        internal int SinkCount
        {
            get { return Volatile.Read(ref _sinks).Length; }
        }

        /// <summary>Lowest ordinal over all registered sinks; null when no sink is registered.</summary>
        internal LogLevel? EffectiveMinimumLevel
        {
            get
            {
                var sinks = Volatile.Read(ref _sinks);
                LogLevel? level = null;

                for (var i = 0; i < sinks.Length; i++)
                {
                    var candidate = sinks[i].MinimumLevel;
                    if (level == null || candidate.Ordinal < level.Ordinal)
                        level = candidate;
                }

                return level;
            }
        }

        /// <summary>Registers a sink. Called under the live lock of <see cref="BGLoggerConfiguration"/>.</summary>
        internal void AddSink(BGLiveSink sink)
        {
            var current = _sinks;
            var updated = new BGLiveSink[current.Length + 1];
            Array.Copy(current, updated, current.Length);
            updated[current.Length] = sink;

            Volatile.Write(ref _sinks, updated);
        }

        /// <summary>Removes a sink, matched by reference. Called under the live lock.</summary>
        internal void RemoveSink(BGLiveSink sink)
        {
            var current = _sinks;
            var index = Array.IndexOf(current, sink);
            if (index < 0)
                return;

            if (current.Length == 1)
            {
                Volatile.Write(ref _sinks, Array.Empty<BGLiveSink>());
                return;
            }

            var updated = new BGLiveSink[current.Length - 1];
            Array.Copy(current, 0, updated, 0, index);
            Array.Copy(current, index + 1, updated, index, current.Length - index - 1);

            Volatile.Write(ref _sinks, updated);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            if (t_inWrite)
                return;                         // a sink that logs cannot recurse

            var sinks = Volatile.Read(ref _sinks);
            if (sinks.Length == 0)
                return;

            t_inWrite = true;
            try
            {
                var record = new BGLogRecord(logEvent.TimeStamp, logEvent.Level, logEvent.LoggerName,
                                             RenderLogEvent(Layout, logEvent), logEvent.Exception?.ToString());

                for (var i = 0; i < sinks.Length; i++)
                {
                    var sink = sinks[i];
                    if (logEvent.Level.Ordinal < sink.MinimumLevel.Ordinal)
                        continue;

                    try
                    {
                        sink.Callback(record);
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Warn(ex, "BGLiveTarget: a live sink threw an exception.");
                    }
                }
            }
            finally
            {
                t_inWrite = false;
            }
        }
    }

    /// <summary>
    /// Registration handle returned by
    /// <see cref="BGLoggerConfiguration.AddLiveSink(Action{BGLogRecord}, LogLevel)"/>.
    /// Disposing removes the sink; <see cref="Dispose"/> is idempotent and thread-safe, but must not be
    /// called from inside a live-sink callback.
    /// </summary>
    internal sealed class BGLiveSink : IDisposable
    {
        private BGLoggerConfiguration? _owner;

        internal BGLiveSink(BGLoggerConfiguration owner, Action<BGLogRecord> callback, LogLevel minimumLevel)
        {
            _owner = owner;
            Callback = callback;
            MinimumLevel = minimumLevel;
        }

        internal Action<BGLogRecord> Callback { get; }

        internal LogLevel MinimumLevel { get; }

        /// <exception cref="InvalidOperationException">
        /// Called from inside a live-sink callback. Removing a registration takes the configuration's live
        /// lock while the callback already holds NLog's per-target lock, so it would deadlock against any
        /// other thread registering or removing a sink. Failing fast keeps the handle valid: dispose it from
        /// the thread that registered it instead.
        /// </exception>
        public void Dispose()
        {
            if (BGLiveTarget.IsInWrite)
                throw new InvalidOperationException(
                    "A live-sink registration must not be disposed from inside a live-sink callback. " +
                    "Dispose it from the thread that owns the registration.");

            var owner = Interlocked.Exchange(ref _owner, null);
            owner?.RemoveLiveSink(this);
        }
    }
}
