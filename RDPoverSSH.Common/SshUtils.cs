using System;
using System.IO;
using System.Linq;

namespace RDPoverSSH.Common
{
    /// <summary>
    /// A collection of utilitiy methods related to SSH
    /// </summary>
    public static class SshUtils
    {
        /// <summary>
        /// Gets the value of a config setting with the given <paramref name="name"/> from the ssh_config file
        /// and tries to convert it to <typeparamref name="T"/>.
        /// Optionally specify a default value and/or get the value even if it is commented out.
        /// </summary>
        public static T GetConfigValue<T>(string name, T defaultValue = default, bool includeCommented = false)
        {
            T result = defaultValue;

            if (File.Exists(Values.SshdConfigFile))
            {
                string match = File.ReadAllLines(Values.SshdConfigFile) // Read the config file
                    .Select(line => includeCommented ? line.TrimStart('#') : line) // Optionally include commented lines as valid matches
                    .FirstOrDefault(line => line.StartsWith(name))? // Find the line starting with the given name
                    .Split().LastOrDefault(); // Split on space and return the last value

                try
                {
                    result = (T)Convert.ChangeType(match, typeof(T));
                }
                catch
                {
                    // Swallow, we already have a default.
                }
            }

            return result;
        }
    }
}
