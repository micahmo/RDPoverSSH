using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using CommandLine;
using Jot;
using Jot.Storage;
using RDPoverSSH.Arguments;
using RDPoverSSH.Common;
using RDPoverSSH.Controls;
using Application = System.Windows.Application;

namespace RDPoverSSH
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <inheritdoc/>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Any())
            {
                HasArgs = true;
                ParsedArgs = Parser.Default.ParseArguments<ShowMessageArgument, SaveKeyArgument, DeleteUserArgument>(e.Args);

                // Handle some args here, without ever going to the UI
                ParsedArgs.WithParsed<DeleteUserArgument>(arg =>
                {
                    UserUtils.DeleteWindowsUser(arg.Username);
                    Environment.Exit(0);
                });
            }
            else
            {
                // See if we're already running, and if so focus that instance.
                // But only do so if we're a UI instance and not a CLI instance (as indicated by no args).
                if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).FirstOrDefault(p => p.Id != Process.GetCurrentProcess().Id) is { } existingProcess)
                {
                    SetForegroundWindow(existingProcess.MainWindowHandle);
                    Environment.Exit(0);
                }
            }

            // Handle global exceptions
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Set up state tracking
            Tracker.Configure<Window>()
                .Id(w => w.Name, SystemInformation.VirtualScreen.Size)
                .Properties(w => new {w.Top, w.Width, w.Height, w.Left, w.WindowState})
                .PersistOn(nameof(Window.Closing))
                .StopTrackingOn(nameof(Window.Closing))
                .WhenPersistingProperty((w, p) => p.Cancel = p.Property == nameof(w.WindowState) && w.WindowState == WindowState.Minimized);
        }

        public static bool HasArgs { get; private set; }

        public static ParserResult<object> ParsedArgs { get; private set; }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Dispatcher.Invoke(async () => await MessageBoxHelper.ShowCopyableText(RDPoverSSH.Properties.Resources.UnexpectedError, RDPoverSSH.Properties.Resources.Error, e.Exception.ToString()));

            // Don't kill the app
            e.Handled = true;
        }

        public static Tracker Tracker { get; } = new Tracker(new JsonFileStore(Values.RoamingApplicationDataPath));

        #region P/Invoke

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion
    }
}
