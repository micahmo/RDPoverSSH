using CommandLine;

namespace RDPoverSSH.Arguments
{
    [Verb("showmessage")]
    public class ShowMessageArgument
    {
        [Value(0)]
        public string Text { get; set; }

        /// <summary>
        /// Request the SshServerPrivateKey to be shown in a message
        /// </summary>
        public static string SshServerPrivateKey = "private_key";

    }
}
