
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace RDPoverSSH
{
    public abstract class MyObservableObject : ObservableObject
    {
        /// <summary>
        /// Exposes the protected <see cref="ObservableObject.OnPropertyChanged(string?)"/>.
        /// </summary>
        /// <param name="propertyName"></param>
        public void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
    }
}
