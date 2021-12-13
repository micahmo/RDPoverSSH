using ModernWpf.Controls;
using RDPoverSSH.ViewModels;

namespace RDPoverSSH.Views
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : ContentDialog
    {
        public SettingsDialog()
        {
            InitializeComponent();
            DataContext = new SettingsDialogViewModel();
        }
    }
}
