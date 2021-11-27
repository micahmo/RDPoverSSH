using System;
using System.ComponentModel;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LinqKit;
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
            MainWindowViewModel viewModel = new MainWindowViewModel();
            DataContext = viewModel;

            try
            {
                RootModel.Instance.Load(PredicateBuilder.New<ConnectionModel>(true));
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show("There was an error accessing the database configuration file. Shutting down.", "RDPoverSSH Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }

            viewModel.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName.Equals(nameof(MainWindowViewModel.Filter)))
                {
                    ExpressionStarter<ConnectionModel> predicate = PredicateBuilder.New<ConnectionModel>(true);

                    if (!string.IsNullOrEmpty(viewModel.Filter))
                    {
                        predicate = predicate.And(i => i.Name.Contains(viewModel.Filter));
                    }

                    RootModel.Instance.Load(predicate);
                }
            };

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
