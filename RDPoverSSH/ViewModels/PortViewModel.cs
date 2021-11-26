using RDPoverSSH.Properties;

namespace RDPoverSSH.ViewModels
{
    public class PortViewModel
    {
        public static PortViewModel Custom { get; } = new CustomPortViewModel();

        public int Value { get; set; }

        public string Name { get; set; }

        public virtual string DisplayName => $"{Value} ({Name})";
    }

    internal class CustomPortViewModel : PortViewModel
    {
        public override string DisplayName => Resources.Custom;
    }
}
