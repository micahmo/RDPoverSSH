using System;
using System.IO;

namespace RDPoverSSH.Common
{
    /// <summary>
    /// A collection of values shared by the server and client
    /// </summary>
    public static class Values
    {
        public static readonly string SshProgramDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        public static readonly string OurPrivateKeyFilePath = Path.Combine(SshProgramDataPath, "ssh", "ssh_rdp_over_ssh_key");
        public static readonly string OurPublicKeyFilePath = Path.Combine(SshProgramDataPath, "ssh", "ssh_rdp_over_ssh_key.pub");
        public static readonly string AdministratorsAuthorizedKeysFilePath = Path.Combine(SshProgramDataPath, "ssh", "administrators_authorized_keys");
    }
}
