namespace RDPoverSSH.Models
{
    public interface ISettingModel
    {
        string Value { get; set; }

        void Save();
    }
}
