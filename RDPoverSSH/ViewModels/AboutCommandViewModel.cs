using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Bluegrams.Application.WPF;
using Microsoft.Toolkit.Mvvm.Input;
using RDPoverSSH.Common;
using RDPoverSSH.Properties;
using RDPoverSSH.Utilities;

namespace RDPoverSSH.ViewModels
{
    public class AboutCommandViewModel : CommandViewModelBase
    {
        #region CommandViewModelBase members

        /// <inheritdoc/>
        public override string Name => Resources.About;

        /// <inheritdoc/>
        public override ICommand Command => _command ??= new RelayCommand(() =>
        {
            var aboutBox = new AboutBox(Application.Current.MainWindow?.Icon, showLanguageSelection: false)
            {
                Owner = Application.Current.MainWindow,
                UpdateChecker = new MyUpdateChecker(Values.VersionInfoUrl, Application.Current.MainWindow)
            };

            // Have to use reflection hack to change the font size
            (aboutBox.GetType().GetField("butUpdate", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(aboutBox) as Button ?? new Button()).FontSize = 12;

            aboutBox.ShowDialog();
        });
        private ICommand _command;

        /// <inheritdoc/>
        public override string IconGlyph => Icons.Info;

        #endregion
    }
}
