using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace RDPoverSSH.ViewModels
{
    /// <summary>
    /// The base class for commands
    /// </summary>
    public abstract class CommandViewModelBase : ObservableObject
    {
        /// <summary>
        /// The user-friendly name of the command
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The command to execute
        /// </summary>
        public abstract ICommand Command { get; }

        /// <summary>
        /// The icon associated with the command
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font
        /// </remarks>
        public abstract string IconGlyph { get; }
    }
}
