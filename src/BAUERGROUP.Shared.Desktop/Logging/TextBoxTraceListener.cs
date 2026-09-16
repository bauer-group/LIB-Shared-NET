#nullable enable

using System.Diagnostics;
using System.Windows;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace BAUERGROUP.Shared.Desktop.Logging;

/// <summary>
/// Trace listener that writes messages to a WPF TextBox.
/// </summary>
[Obsolete("Unused, and blocks the writing thread through Dispatcher.Invoke. Use LogMessageReceiverControl or " +
          "BGLogger.Configuration.AddLiveSink / BGLogViewBuffer. This type will be removed in a future major version.")]
public class TextBoxTraceListener : TraceListener
{
    private readonly WpfTextBox _target;
    private readonly Window _windowHost;

    /// <summary>
    /// Creates a new TextBox trace listener.
    /// </summary>
    /// <param name="target">The TextBox to write to.</param>
    /// <param name="windowHost">The window hosting the TextBox.</param>
    public TextBoxTraceListener(WpfTextBox target, Window windowHost)
    {
        _target = target;
        _windowHost = windowHost;
    }

    /// <inheritdoc />
    public override void Write(string? message)
    {
        Append(message);
    }

    /// <inheritdoc />
    public override void WriteLine(string? message)
    {
        Append(message);
    }

    private void Append(string? message)
    {
        _windowHost.Dispatcher.Invoke(() =>
        {
            _target.Text += $"{DateTime.UtcNow} {message}{Environment.NewLine}";
            _target.ScrollToEnd();
        });
    }
}
