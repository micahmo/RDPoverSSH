using CommandLine;

namespace RDPoverSSH.Arguments
{
    [Verb("savekey")]
    public class SaveKeyArgument
    {
        [Value(0)]
        public string KeyId { get; set; }

        [Value(1)]
        public int ConnectionId { get; set; }

        /// <summary>
        /// The identifier for the private key
        /// </summary>
        public static string SshServerPrivateKey = "private_key";
    }
}
