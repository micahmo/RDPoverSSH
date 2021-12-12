using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommandLine;
using CommandLine.Text;
using RDPoverSSH.Arguments;
using RDPoverSSH.Common;
using RDPoverSSH.Controls;
using RDPoverSSH.DataStore;
using RDPoverSSH.Models;
using RDPoverSSH.ViewModels;

namespace RDPoverSSH.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (App.HasArgs)
            {
                App.ParsedArgs
                    .WithParsed<ShowMessageArgument>(ShowMessage)
                    .WithParsed<SaveKeyArgument>(SaveKey)
                    .WithNotParsed(_ =>
                    {
                        HelpText.AutoBuild(App.ParsedArgs);
                        Environment.Exit(0);
                    });

                RootControl.Visibility = Visibility.Hidden;
                WindowStyle = WindowStyle.None;
                Width = 500;
                Height = 500;
                return;
            }

            MainWindowViewModel viewModel = new MainWindowViewModel();
            DataContext = viewModel;

            // Make sure the worker service is installed and running.
            Task.Run(async () =>
            {
                ServiceControllerStatus? status = null;

                try
                {
                    ServiceController rdpOverSshWorkerService = new ServiceController {ServiceName = "RDPoverSSH.Service.Worker" };
                    status = rdpOverSshWorkerService.Status;
                }
                catch
                {
                    // Swallow the exception
                }

                if (status is null)
                {
                    await Dispatcher.Invoke(() => MessageBoxHelper.Show(Properties.Resources.ServiceNotInstalled, Properties.Resources.Error, MessageBoxButton.OK));
                }
                else if (status != ServiceControllerStatus.Running)
                {
                    await Dispatcher.Invoke(() => MessageBoxHelper.Show(Properties.Resources.ServiceNotRunning, Properties.Resources.Error, MessageBoxButton.OK));
                }
            });

            // Hook up some object updates
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(5)).Subscribe(_ =>
            {
                // Database access should be made on the UI thread
                Dispatcher.Invoke(() =>
                {
                    foreach (ConnectionViewModel connectionViewModel in viewModel.Connections.ToList())
                    {
                        if (DatabaseEngine.GetCollection<ConnectionServiceModel>().FindById(connectionViewModel.Model.ObjectId) is { } connection
                            && connection.Direction == connectionViewModel.Model.TunnelDirection 
                            && connection.LastStatusUpdateDateTime > connectionViewModel.LastStatusUpdateDateTime) // Make sure we don't get stale updates
                        {
                            if (connectionViewModel.Status != connection.Status)
                            {
                                connectionViewModel.Status = connection.Status;
                            }

                            if (connectionViewModel.LastError != connection.TimestampedLastError)
                            {
                                connectionViewModel.LastError = connection.TimestampedLastError;
                            }

                            if (connectionViewModel.Model.TunnelDirection == Direction.Outgoing)
                            {
                                connectionViewModel.RemoteMachineName = string.IsNullOrWhiteSpace(connection.RemoteMachineName)
                                    ? Properties.Resources.RemoteComputer
                                    : string.Format(Properties.Resources.RemoteComputerName, connection.RemoteMachineName);
                            }
                        }
                    }
                });
            });

            // Make all tooltips stay open
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata((int)TimeSpan.FromSeconds(30).TotalMilliseconds));
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Need to do this hear instead of in the constructor to handle multi-window.
            // https://github.com/anakic/Jot/issues/3
            App.Tracker.Track(this);
        }

        private void ShowMessage(ShowMessageArgument arg)
        {
            Loaded += async (_, __) =>
            {
                // Figure out which message to show
                if (arg.Text.Equals(ShowMessageArgument.SshServerPrivateKey))
                {
                    try
                    {
                        string privateKeyText = await File.ReadAllTextAsync(Values.OurPrivateKeyFilePath);
                        await MessageBoxHelper.ShowCopyableText(Properties.Resources.SshPrivateKeyDescription, Properties.Resources.SshServerKeyHeading, privateKeyText, monospace: true);
                    }
                    catch (Exception)
                    {
                        await MessageBoxHelper.Show(Properties.Resources.ErrorGettingPrivateKey, Properties.Resources.Error, MessageBoxButton.OK);
                    }
                }
                else
                {
                    await MessageBoxHelper.Show(arg.Text, "RDPoverSSH", MessageBoxButton.OK);
                }

                Environment.Exit(0);
            };
        }

        private void SaveKey(SaveKeyArgument args)
        {
            Loaded += async (_, __) =>
            {
                // Figure out which key to save
                if (args.KeyId.Equals(SaveKeyArgument.SshServerPrivateKey))
                {
                    try
                    {
                        string privateKey = await MessageBoxHelper.ShowPastableText(Properties.Resources.PasteSshServerPrivateKey, Properties.Resources.SshServerKeyHeading, monospace: true);
                        if (!string.IsNullOrEmpty(privateKey))
                        {
                            FileUtils.CreateFileWithSecureAcl(Values.ClientServerPrivateKeyFilePath(args.ConnectionId));

                            // Trim it and add a single trailing Linux newline
                            privateKey = $"{privateKey.Trim()}\n";
                            await File.WriteAllTextAsync(Values.ClientServerPrivateKeyFilePath(args.ConnectionId), privateKey);
                        }
                    }
                    catch (Exception)
                    {
                        await MessageBoxHelper.Show(Properties.Resources.ErrorSavingPrivateKey, Properties.Resources.Error, MessageBoxButton.OK);
                    }
                }

                Environment.Exit(0);
            };
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            DatabaseEngine.Shutdown();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }
    }
}
