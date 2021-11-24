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
                OnPropertyChanged(nameof(Connections));
            };
        }

        #endregion

        /// <summary>
        /// The list of commands to show on the main window
        /// </summary>
        public List<CommandViewModelBase> Commands => new List<CommandViewModelBase>
        {
            new SettingsCommandViewModel(),
            new NewConnectionCommandViewModel()
        };

        public RootModel Model => RootModel.Instance;

        public List<ConnectionViewModel> Connections => Model.Connections.Select(c => new ConnectionViewModel {Model = c}).ToList();
    }
}
