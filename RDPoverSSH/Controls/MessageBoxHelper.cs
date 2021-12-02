using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
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

        private static FrameworkElement GetTextBlockContent(string message, string textBlock, bool monospace, bool readOnly, out FlowDocument document)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap
            });

            RichTextBox richTextBox = new RichTextBox
            {
                IsReadOnly = readOnly,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                // This is needed in order for the vertical scrollbar to appear
                Height = 300,
                Margin = new Thickness(0, 5, 0, 5),
            };

            if (monospace)
            {
                richTextBox.FontFamily = new FontFamily("Courier New");
            }

            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new Paragraph(new Run(textBlock)));
            // This is needed in order for the horizontal scrollbar to appear
            richTextBox.Document.Blocks.FirstBlock.LineHeight = 1;
            richTextBox.Document.PageWidth = 1000;
            stackPanel.Children.Add(richTextBox);

            document = richTextBox.Document;
            return stackPanel;
        }

        public static async Task ShowCopyableText(string message, string title, string textBlock, bool monospace = false)
        {
            var stackPanel = GetTextBlockContent(message, textBlock, monospace, readOnly: true, out _);

            ContentDialog contentDialog = new ContentDialog
            {
                Title = title,
                Content = stackPanel,
                DefaultButton = ContentDialogButton.Primary,
                PrimaryButtonText = Resources.Copy,
                CloseButtonText = Resources.Close
            };

            if (await contentDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                Clipboard.SetText(textBlock);
            }
        }

        public static async Task<string> ShowPastableText(string message, string title, bool monospace = false)
        {
            var stackPanel = GetTextBlockContent(message, string.Empty, monospace, readOnly: false, out var flowDocument);

            ContentDialog contentDialog = new ContentDialog
            {
                Title = title,
                Content = stackPanel,
                DefaultButton = ContentDialogButton.Primary,
                PrimaryButtonText = Resources.Save,
                CloseButtonText = Resources.Cancel
            };

            if (await contentDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                return new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd).Text;
            }

            return default;
        }
    }
}
