using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls;
using RDPoverSSH.Properties;

namespace RDPoverSSH.Controls
{
    /// <summary>
    /// Contains static methods for showing MessageBoxes using ModernWpf
    /// </summary>
    public static class MessageBoxHelper
    {
        /// <summary>
        /// Show a MessageBox with the given options
        /// </summary>
        public static Task<ContentDialogResult> Show(string message, string title, MessageBoxButton buttons)
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = title,
                Content = new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 300
                },
                DefaultButton = ContentDialogButton.Primary
            };

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    contentDialog.PrimaryButtonText = Resources.OK;
                    break;
                case MessageBoxButton.OKCancel:
                    contentDialog.PrimaryButtonText = Resources.OK;
                    contentDialog.SecondaryButtonText = Resources.Cancel;
                    break;
                case MessageBoxButton.YesNo:
                    contentDialog.PrimaryButtonText = Resources.Yes;
                    contentDialog.SecondaryButtonText = Resources.No;
                    break;
                case MessageBoxButton.YesNoCancel:
                    contentDialog.PrimaryButtonText = Resources.Yes;
                    contentDialog.SecondaryButtonText = Resources.No;
                    contentDialog.CloseButtonText = Resources.Cancel;
                    break;
            }

            return contentDialog.ShowAsync();
        }
    }
}
