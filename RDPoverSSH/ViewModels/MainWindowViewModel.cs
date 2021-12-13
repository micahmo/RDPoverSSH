using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using LinqKit;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using RDPoverSSH.DataStore;
using RDPoverSSH.Models;
using RDPoverSSH.Properties;

namespace RDPoverSSH.ViewModels
{
    /// <summary>
    /// The MainWindow's model
    /// </summary>
    public class MainWindowViewModel : ObservableObject
    {
        #region Constructor

        public MainWindowViewModel()
        {
            RootModel.Instance.Connections.CollectionChanged += (_, __) =>
            {
                Connections = RootModel.Instance.Connections.Select(c => new ConnectionViewModel(c)).ToList();
                OnPropertyChanged(nameof(Connections));
                OnPropertyChanged(nameof(ShowFilter));
                OnPropertyChanged(nameof(ShowNoResultsHint));
            };

            try
            {
                // This is the first database access, so here's where we'll catch db file load issues.
                Model = DatabaseEngine.GetCollection<MainWindowModel>().Query().FirstOrDefault() ?? new MainWindowModel();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show(Resources.ErrorAccessingDatabase, Resources.RdpOverSshError, MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }

            Model.PropertyChanged += (_, args) =>
            {
                // Any time the model changes, persist it
                DatabaseEngine.GetCollection<MainWindowModel>().Upsert(Model);

                if (args.PropertyName.Equals(nameof(Model.Filter)))
                {
                    OnPropertyChanged(nameof(ShowFilter));

                    // Redo the filter
                    RootModel.Instance.Load(GeneratePredicate());
                }
            };

            // Do the initial load.
            RootModel.Instance.Load(GeneratePredicate());
        }

        public MainWindowModel Model { get; }

        #endregion

        /// <summary>
        /// The list of commands to show on the main window
        /// </summary>
        public List<CommandViewModelBase> Commands => new List<CommandViewModelBase>
        {
            new AboutCommandViewModel(),
            new SettingsCommandViewModel(),
            //new ImportConnectionCommandViewModel(),
            new NewConnectionCommandViewModel()
        };

        public List<ConnectionViewModel> Connections { get; private set; } = new List<ConnectionViewModel>();

        public bool ShowFilter => Connections.Count > 0 || !string.IsNullOrEmpty(Model.Filter);

        public bool ShowNoResultsHint => Connections.Count == 0 && !string.IsNullOrEmpty(Model.Filter);

        #region Private methods

        private ExpressionStarter<ConnectionModel> GeneratePredicate()
        {
            ExpressionStarter<ConnectionModel> predicate = PredicateBuilder.New<ConnectionModel>(true);

            if (!string.IsNullOrEmpty(Model.Filter))
            {
                predicate = predicate.And(i => i.Name.Contains(Model.Filter));
            }

            return predicate;
        }

        #endregion
    }
}
