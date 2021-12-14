using System;
using RDPoverSSH.Models;

namespace RDPoverSSH.ViewModels.Settings
{
    /// <summary>
    /// The UI representation of a Setting
    /// </summary>
    public abstract class SettingViewModelBase : MyObservableObject
    {
        #region Public properties

        /// <summary>
        /// A user-translated representation of the setting name
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        private string _name;

        /// <summary>
        /// A user-translated representation of the category. Use identical categories to group settings.
        /// </summary>
        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }
        private string _category;

        /// <summary>
        /// A user-translated description of the setting (tooltip)
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
        private string _description;

        public abstract bool IsBinary { get; }

        #endregion
    }

    /// <inheritdoc/>
    public class SettingViewModel<TValue, TSetting> : SettingViewModelBase where TSetting : MyObservableObject, ISettingModel
    {
        #region Constructor

        public SettingViewModel(TSetting setting, string category, string name, TValue defaultValue, string description = default)
        {
            _model = setting;
            Name = name;
            Category = category;
            Description = description;
            _defaultValue = defaultValue;

            setting.PropertyChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(Value));
            };
        }

        #endregion

        #region SettingViewModelBase members

        public override bool IsBinary => typeof(bool).IsAssignableFrom(typeof(TValue));

        #endregion

        #region Public properties

        public TValue Value
        {
            get
            {
                try
                {
                    return (TValue)Convert.ChangeType(_model.Value, typeof(TValue));
                }
                catch
                {
                    return _defaultValue;
                }
            }
            set
            {
                _model.Value = value.ToString();
                _model.Save();
                OnPropertyChanged(nameof(Value));
            }
        }

        #endregion

        #region Private fields

        private readonly TSetting _model;
        private readonly TValue _defaultValue;

        #endregion
    }
}
