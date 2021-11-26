using System.Windows;
using System.Windows.Controls;

namespace RDPoverSSH.Views
{
    /// <summary>
    /// Interaction logic for CommandButton.xaml
    /// </summary>
    public partial class CommandButton : UserControl
    {
        #region Dependency properties

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(CommandButton));

        #endregion

        #region Constructor

        public CommandButton()
        {
            InitializeComponent();
        }

        #endregion

        #region Bindable properties

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        #endregion
    }
}
