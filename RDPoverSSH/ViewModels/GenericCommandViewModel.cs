using System.Windows.Input;

namespace RDPoverSSH.ViewModels
{
    /// <summary>
    /// This class allows constructing a command rather than defining it in code
    /// </summary>
    public class GenericCommandViewModel : CommandViewModelBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public GenericCommandViewModel(string name, ICommand command, string iconGlyph, string description = default)
        {
            Name = name;
            Command = command;
            IconGlyph = iconGlyph;
            Description = description;
        }

        #endregion
        
        /// <inheritdoc/>
        public override string Name { get; }

        /// <inheritdoc/>
        public override string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
        private string _description;

        /// <inheritdoc/>
        public override ICommand Command { get; }

        /// <inheritdoc/>
        public override string IconGlyph
        {
            get => _iconGlyph;
            set => SetProperty(ref _iconGlyph, value);
        }

        private string _iconGlyph;
    }
}
