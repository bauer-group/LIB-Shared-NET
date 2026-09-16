using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Linq;

namespace BAUERGROUP.Shared.Avalonia.Logging
{
    /// <summary>
    /// Optional window hosting a <see cref="LogViewer"/>. Uses only portable window semantics: it is shown
    /// with an owner, never sets Topmost or Position and never calls Activate() - native Wayland ignores
    /// those. Escape closes it; closing the owner closes it too, which detaches the viewer and unregisters
    /// the live sink.
    /// </summary>
    public class LogViewerWindow : Window
    {
        /// <summary>Creates the window with the default title "Log Viewer".</summary>
        public LogViewerWindow()
            : this(null)
        {
        }

        /// <summary>Creates the window.</summary>
        /// <param name="title">Window title; null or empty means "Log Viewer".</param>
        public LogViewerWindow(String? title)
        {
            Title = String.IsNullOrEmpty(title) ? "Log Viewer" : title;
            Width = 960;
            Height = 540;
            MinWidth = 480;
            MinHeight = 240;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            Viewer = new LogViewer();
            Content = Viewer;
        }

        /// <summary>The hosted viewer. Configure <see cref="LogViewer.MaxLines"/> and
        /// <see cref="LogViewer.MinimumLevel"/> through it.</summary>
        public LogViewer Viewer { get; }

        /// <summary>
        /// Opens a log window owned by <paramref name="owner"/>, or closes the one already owned by it.
        /// Ideal for an F12 handler.
        /// </summary>
        /// <param name="owner">Window that owns the log window.</param>
        /// <param name="title">Window title; null means "Log Viewer".</param>
        /// <returns>The opened window, or null when an existing one was closed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="owner"/> is null.</exception>
        public static LogViewerWindow? Toggle(Window owner, String? title = null)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            // No static state: ownership is read back from the owner itself, so several owners can each have
            // their own log window and a closed window leaves nothing behind.
            var existing = owner.OwnedWindows.OfType<LogViewerWindow>().FirstOrDefault();

            if (existing != null)
            {
                existing.Close();
                return null;
            }

            var window = new LogViewerWindow(title);
            window.Show(owner);
            return window;
        }

        /// <summary>Escape closes the window.</summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Handled || e.Key != Key.Escape)
                return;

            e.Handled = true;
            Close();
        }
    }
}
