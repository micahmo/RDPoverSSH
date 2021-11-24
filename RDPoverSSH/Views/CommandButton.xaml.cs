using System.Windows;
using System.Windows.Controls;

namespace RDPoverSSH.Views
{
    /// <summary>
    /// Interaction logic for CommandButton.xaml
    /// </summary>
    public partial class CommandButton : UserControl
    {
        public CommandButton()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(CommandButton));

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }
    }
}
