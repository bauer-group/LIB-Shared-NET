using NLog;
using System;
using System.Globalization;

namespace BAUERGROUP.Shared.Core.Logging
{
    /// <summary>
    /// Immutable snapshot of one log event delivered to in-process live sinks
    /// (see <see cref="BGLoggerConfiguration.AddLiveSink(Action{BGLogRecord}, LogLevel)"/>).
    /// Holds no reference to the NLog <c>LogEventInfo</c> and no reference to the <see cref="System.Exception"/>
    /// object, so a full viewer buffer cannot pin object graphs.
    /// </summary>
    public sealed class BGLogRecord
    {
        /// <summary>
        /// Maximum number of characters kept for <see cref="Message"/> and for <see cref="Exception"/>
        /// individually. Longer text is cut and marked with <see cref="TruncationMarker"/>; the full text is still
        /// written by the FILE target. Bounds a viewer at MaxLines * 2 * 4096 characters.
        /// </summary>
        public const int MaxTextLength = 4096;

        /// <summary>Appended to text cut at <see cref="MaxTextLength"/>. Value: " …[truncated]".</summary>
        public const string TruncationMarker = " …[truncated]";

        // Lazy on purpose: virtualization realizes only the visible rows, and records trimmed from the
        // buffer must never cost a format. The double-compute race is benign (idempotent, and a
        // reference assignment is atomic).
        private string? _displayText;

        /// <summary>Creates an immutable snapshot of one log event.</summary>
        /// <param name="timeStamp">Event time as produced by NLog.</param>
        /// <param name="level">NLog level of the event.</param>
        /// <param name="loggerName">Name of the originating logger; null becomes an empty string.</param>
        /// <param name="message">Rendered message; null becomes an empty string.</param>
        /// <param name="exception">Exception text; null when the event carried no exception.</param>
        /// <exception cref="ArgumentNullException"><paramref name="level"/> is null.</exception>
        public BGLogRecord(DateTime timeStamp, LogLevel level, string? loggerName,
                           string? message, string? exception = null)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));

            TimeStamp = timeStamp;
            Level = level;
            LoggerName = loggerName ?? String.Empty;
            Message = Truncate(message) ?? String.Empty;
            Exception = Truncate(exception);
            IsWarning = level.Ordinal == LogLevel.Warn.Ordinal;
            IsError = level.Ordinal >= LogLevel.Error.Ordinal;
        }

        /// <summary>
        /// Event time as produced by NLog (local time, <c>DateTimeKind.Local</c>). NLog's default
        /// time source is cached to roughly 15 ms, so equal timestamps are common — list order is authoritative.
        /// </summary>
        public DateTime TimeStamp { get; }

        /// <summary>NLog level. <c>Level.Ordinal</c> runs 0 (Trace) .. 5 (Fatal).</summary>
        public LogLevel Level { get; }

        /// <summary>
        /// Name of the originating logger; empty string when unknown. Not part of
        /// <see cref="DisplayText"/> — BGLogger uses a single logger named after the entry assembly.
        /// </summary>
        public string LoggerName { get; }

        /// <summary>Rendered message, truncated at <see cref="MaxTextLength"/>.</summary>
        public string Message { get; }

        /// <summary>
        /// <c>Exception.ToString()</c> captured on the logging thread, truncated at
        /// <see cref="MaxTextLength"/>; null when the event carried no exception.
        /// </summary>
        public string? Exception { get; }

        /// <summary><c>Level == LogLevel.Warn</c>. Exists so XAML can bind a style class without a converter.</summary>
        public bool IsWarning { get; }

        /// <summary><c>Level.Ordinal &gt;= LogLevel.Error.Ordinal</c> (Error and Fatal).</summary>
        public bool IsError { get; }

        /// <summary>
        /// "HH:mm:ss.fff LEVEL Message" (invariant culture, level upper-case padded to 5), followed by
        /// the exception text on the next lines when present. Computed on first access and cached.
        /// </summary>
        public string DisplayText
        {
            get
            {
                var displayText = _displayText;
                if (displayText == null)
                {
                    displayText = BuildDisplayText();
                    _displayText = displayText;
                }

                return displayText;
            }
        }

        /// <summary>Returns <see cref="DisplayText"/>.</summary>
        public override string ToString()
        {
            return DisplayText;
        }

        private string BuildDisplayText()
        {
            var text = String.Concat(
                TimeStamp.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture),
                " ",
                Level.Name.ToUpperInvariant().PadRight(5),
                " ",
                Message);

            return Exception == null ? text : text + Environment.NewLine + Exception;
        }

        private static string? Truncate(string? value)
        {
            if (value == null)
                return null;

            return value.Length <= MaxTextLength ? value : value.Substring(0, MaxTextLength) + TruncationMarker;
        }
    }
}
