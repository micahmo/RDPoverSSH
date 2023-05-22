using Microsoft.Toolkit.Mvvm.Input;
using RDPoverSSH.Common;
using System.Windows.Input;
using RDPoverSSH.Properties;

namespace RDPoverSSH.ViewModels
{
    internal class ExpandAllConnectionsCommandViewModel : CommandViewModelBase
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        
        public ExpandAllConnectionsCommandViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }

        public override string Name => string.Empty;

        public override string Description => Resources.ExpandAllConnectionsToolTip;

        public override ICommand Command => _command ??= new RelayCommand(() =>
        {
            _mainWindowViewModel.Connections.ForEach(c => c.Model.IsInEditMode = true);
        });
        private ICommand _command;

        public override string IconGlyph => Icons.ExpandAll;
    }

    internal class CollapseAllConnectionsCommandViewModel : CommandViewModelBase
    {
        private readonly MainWindowViewModel _mainWindowViewModel;

        public CollapseAllConnectionsCommandViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }

        public override string Name => string.Empty;

        public override string Description => Resources.CollapseAllConnectionsToolTip;

        public override ICommand Command => _command ??= new RelayCommand(() =>
        {
            _mainWindowViewModel.Connections.ForEach(c => c.Model.IsInEditMode = false);
        });
        private ICommand _command;

        public override string IconGlyph => Icons.CollapseAll;
    }
}
