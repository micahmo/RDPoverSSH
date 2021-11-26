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
        /// A description about this command (shown in a tooltip)
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// The command to execute
        /// </summary>
        public abstract ICommand Command { get; }

        /// <summary>
        /// The icon associated with the command
        /// </summary>
        /// <remarks>
        /// <see href="https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font"/>
        /// </remarks>
        public virtual string IconGlyph { get; set; }

        /// <summary>
        /// Whether or not this command has both name and glyph
        /// </summary>
        /// <remarks>
        /// Useful for binding
        /// </remarks>
        public bool HasParts => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrEmpty(IconGlyph);
    }
}
