using System;
using System.IO;
using System.Timers;
using System.Windows;
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
                RootModel.Instance.Load();
            }
            catch (IOException)
            {
                MessageBox.Show("There was an error accessing the database configuration file. Shutting down.", "RDPoverSSH Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }

            Timer autoSaveTimer = new Timer { Interval = TimeSpan.FromMinutes(1).TotalMilliseconds };
            autoSaveTimer.Elapsed += AutoSaveTimer_Elapsed;
            autoSaveTimer.Start();

        }

        private void AutoSaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RootModel.Instance.Save();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RootModel.Instance.Save();
            DatabaseEngine.Shutdown();
        }
    }
}
