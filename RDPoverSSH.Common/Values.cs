using System;
using System.IO;

namespace RDPoverSSH.Common
{
    /// <summary>
    /// A collection of values shared by the server and client
    /// </summary>
    public static class Values
    {
        public static readonly string SshProgramDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ssh");
        public static readonly string OurPrivateKeyFilePath = Path.Combine(SshProgramDataPath, "ssh_rdp_over_ssh_key");
        public static readonly string OurPublicKeyFilePath = Path.Combine(SshProgramDataPath, "ssh_rdp_over_ssh_key.pub");
        public static readonly string AdministratorsAuthorizedKeysFilePath = Path.Combine(SshProgramDataPath, "administrators_authorized_keys");
        public static readonly string RdpOverSshWindowsUsername = "RDPoverSSH";
        public static readonly string SshdConfigFile = Path.Combine(SshProgramDataPath, "sshd_config");
        public static readonly string ApplicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "RDPoverSSH");
        public static readonly string RoamingApplicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RDPoverSSH");
        public static readonly string VersionInfoUrl = "https://raw.githubusercontent.com/micahmo/RDPoverSSH/main/RDPoverSSH/VersionInfo.xml";

        public static string ClientServerPrivateKeyFilePath(int connectionId) => Path.Combine(SshProgramDataPath, $"ssh_rdp_over_ssh_{connectionId}_key");
        
        public static string MappedAddress(int port) => $"localhost:{port}";
    }
}
