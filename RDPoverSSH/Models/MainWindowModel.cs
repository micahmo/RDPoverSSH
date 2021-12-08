using LiteDB;

namespace RDPoverSSH.Models
{
    /// <summary>
    /// Defines the persistent properties of the MainWindow
    /// </summary>
    public class MainWindowModel : ModelBase<MainWindowModel>
    {
        [BsonId]
        public int ObjectId { get; set; }

        public string Filter
        {
            get => _filter;
            set => SetProperty(ref _filter, value);
        }
        private string _filter;
    }
}
