using CommandLine;

namespace RDPoverSSH.Arguments
{
    [Verb("deleteuser")]
    public class DeleteUserArgument
    {
        [Value(0)]
        public string Username { get; set; }
    }
}
