using RDPoverSSH.Models;

namespace RDPoverSSH.ViewModels
{
    public class ConnectionViewModel
    {
        public ConnectionModel Model { get; set; }

        public DeleteConnectionItemViewModel DeleteConnectionItemViewModel { get; } = new DeleteConnectionItemViewModel();
    }
}
