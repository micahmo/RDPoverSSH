using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using RDPoverSSH.Utilities;

namespace RDPoverSSH.ViewModels
{
    /// <summary>
    /// The base class for commands
    /// </summary>
    public abstract class CommandViewModelBase : MyObservableObject
    {
        /// <summary>
        /// Base constructor
        /// </summary>
        protected CommandViewModelBase()
        {
            PropertyChanged += (_, args) =>
            {
                if (args.PropertyName.Equals(nameof(Name)) || args.PropertyName.Equals(nameof(Description)))
                {
                    OnPropertyChanged(nameof(TooltipText));
                }

                if (args.PropertyName.Equals(nameof(SubCommands)))
                {
                    OnPropertyChanged(nameof(HasSubCommands));
                }
            };
        }

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
        /// The color of the icon
        /// </summary>
        public virtual Color IconColor { get; set; } = ApplicationValues.SystemBaseHighColor;

        public virtual bool HasSubCommandSeparator { get; } = false;

        /// <summary>
        /// Whether or not this command has both name and glyph
        /// </summary>
        /// <remarks>
        /// Useful for binding
        /// </remarks>
        public bool HasParts => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrEmpty(IconGlyph);

        /// <summary>
        /// The text that should be displayed on the tooltip
        /// </summary>
        public string TooltipText => string.IsNullOrWhiteSpace(Description) ? Name : Description;

        public virtual List<CommandViewModelBase> SubCommands { get; } = new List<CommandViewModelBase>();

        public bool HasSubCommands => SubCommands.Any();
    }
}
