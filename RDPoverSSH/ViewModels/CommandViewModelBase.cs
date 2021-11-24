using System.Windows.Input;

namespace RDPoverSSH.ViewModels
{
    /// <summary>
    /// The base class for commands
    /// </summary>
    public abstract class CommandViewModelBase
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

        /// <summary>
        /// Whether or not this command has a user-friendly name
        /// </summary>
        /// <remarks>
        /// Useful for binding
        /// </remarks>
        public bool HasName => !string.IsNullOrWhiteSpace(Name);
    }
}
