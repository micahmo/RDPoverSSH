using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls;
using RDPoverSSH.ViewModels;

namespace RDPoverSSH.Views
{
    /// <summary>
    /// Interaction logic for CommandButton.xaml
    /// </summary>
    public partial class CommandButton : UserControl
    {
        #region Dependency properties

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(CommandButton));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(CommandButton));

        public static readonly DependencyProperty IsSubCommandProperty = DependencyProperty.Register(nameof(IsSubCommand), typeof(bool), typeof(CommandButton));

        public static readonly DependencyProperty ParentSplitButtonProperty = DependencyProperty.Register(nameof(ParentSplitButton), typeof(SplitButton), typeof(CommandButton));

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

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsSubCommandProperty);
            set => SetValue(IsSubCommandProperty, value);
        }

        public bool IsSubCommand
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public SplitButton ParentSplitButton
        {
            get => (SplitButton)GetValue(ParentSplitButtonProperty);
            set => SetValue(ParentSplitButtonProperty, value);
        }

        public ICommand Command => _command ??= new RelayCommand(() =>
        {
            // Recursively hide parent Flyouts. No nice way to do this except via code behind.
            SplitButton splitButton = ParentSplitButton;
            while (splitButton != null)
            {
                splitButton.Flyout.Hide();
                splitButton = ((splitButton.Parent as FrameworkElement)?.Parent as CommandButton)?.ParentSplitButton;
            }

            (DataContext as CommandViewModelBase)?.Command?.Execute(CommandParameter);
        });
        private ICommand _command;

        #endregion

        #region Event handlers

        private void SubCommandsMenu_Opening(object sender, object e)
        {
            (DataContext as CommandViewModelBase)?.RaisePropertyChanged(nameof(CommandViewModelBase.SubCommands));
        }

        #endregion
    }
}
