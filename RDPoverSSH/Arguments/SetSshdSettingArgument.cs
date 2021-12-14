using CommandLine;

namespace RDPoverSSH.Arguments
{
    [Verb("sshsetting")]
    public class SetSshdSettingArgument
    {
        [Value(0)]
        public string Name { get; set; }

        [Value(0)]
        public string Value { get; set; }
    }
}
