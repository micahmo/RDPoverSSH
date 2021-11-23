using System.Collections.Generic;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace RDPoverSSH.ViewModels
{
    /// <summary>
    /// The MainWindow's model
    /// </summary>
    public class MainWindowViewModel : ObservableObject
    {
        /// <summary>
        /// The list of commands to show on the main window
        /// </summary>
        public List<CommandViewModelBase> Commands => new List<CommandViewModelBase>
        {
            new SettingsCommandViewModel(),
            new NewConnectionCommandViewModel()
        };
    }
}
