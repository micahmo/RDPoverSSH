using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls;

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
                    contentDialog.PrimaryButtonText = "OK";
                    break;
                case MessageBoxButton.OKCancel:
                    contentDialog.PrimaryButtonText = "OK";
                    contentDialog.SecondaryButtonText = "Cancel";
                    break;
                case MessageBoxButton.YesNo:
                    contentDialog.PrimaryButtonText = "Yes";
                    contentDialog.SecondaryButtonText = "No";
                    break;
                case MessageBoxButton.YesNoCancel:
                    contentDialog.PrimaryButtonText = "Yes";
                    contentDialog.SecondaryButtonText = "No";
                    contentDialog.CloseButtonText = "Cancel";
                    break;
            }

            return contentDialog.ShowAsync();
        }
    }
}
