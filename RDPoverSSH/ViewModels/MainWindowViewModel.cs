using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using RDPoverSSH.Models;

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
            Model.Connections.CollectionChanged += (_, __) =>
            {
                Connections = Model.Connections.Select(c => new ConnectionViewModel(c)).ToList();
                OnPropertyChanged(nameof(Connections));
                OnPropertyChanged(nameof(ShowFilter));
            };
        }

        #endregion

        /// <summary>
        /// The list of commands to show on the main window
        /// </summary>
        public List<CommandViewModelBase> Commands => new List<CommandViewModelBase>
        {
            new SettingsCommandViewModel(),
            new ImportConnectionCommandViewModel(),
            new NewConnectionCommandViewModel()
        };

        public RootModel Model => RootModel.Instance;

        public List<ConnectionViewModel> Connections { get; private set; } = new List<ConnectionViewModel>();

        public string Filter
        {
            get => _filter;
            set
            {
                SetProperty(ref _filter, value);
                OnPropertyChanged(nameof(ShowFilter));
            }
        }

        private string _filter;

        public bool ShowFilter => Connections.Count > 0 || !string.IsNullOrEmpty(_filter);
    }
}
