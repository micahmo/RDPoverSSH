using RDPoverSSH.Properties;

namespace RDPoverSSH.ViewModels
{
    public class PortViewModel
    {
        public static PortViewModel Custom { get; } = new CustomPortViewModel();

        public int Value { get; set; }

        public string Name { get; set; }

        public virtual string DisplayName => $"{Value} ({Name})";

        #region Public static (default ports)

        public static PortViewModel RdpPort { get; } = new PortViewModel {Value = 3389, Name = "RDP"};
        public static PortViewModel SmbPort { get; } = new PortViewModel {Value = 445, Name = "SMB"};
        public static PortViewModel HttpPort { get; } = new PortViewModel {Value = 80, Name = "HTTP"};
        public static PortViewModel HttpsPort { get; } = new PortViewModel {Value = 443, Name = "HTTPS"};
        public static PortViewModel SshPort { get; } = new PortViewModel {Value = 22, Name = "SSH"};

        #endregion
    }

    internal class CustomPortViewModel : PortViewModel
    {
        public override string DisplayName => Resources.Custom;
    }
}
