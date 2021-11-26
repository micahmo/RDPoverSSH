using System;
using System.IO;
using System.Timers;
using System.Windows;
using LinqKit;
using RDPoverSSH.BusinessLogic;
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

            // TODO: These go in Windows services?
            SshServerWorker.Instance.Start();
            SshClientWorker.Instance.Start();

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
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SshServerWorker.Instance.Stop();
            SshClientWorker.Instance.Stop();

            DatabaseEngine.Shutdown();
        }
    }
}
