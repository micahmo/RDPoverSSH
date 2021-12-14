using System;
using System.Collections.Generic;
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
                    result = (T)Convert.ChangeType(TransformFrom(match), typeof(T));
                }
                catch
                {
                    // Swallow, we already have a default.
                }
            }

            return result;
        }

        /// <summary>
        /// Sets the value of a config setting with the given <paramref name="name"/> in the ssh_config file.
        /// If the value is present but commented, it will be uncommented. If the value is not present, it will be added.
        /// </summary>
        public static void SetConfigValue(string name, object value)
        {
            if (File.Exists(Values.SshdConfigFile))
            {
                List<string> lines = File.ReadAllLines(Values.SshdConfigFile).ToList();
                bool foundExistingSetting = false;
                string lineToAdd = $"{name} {TransformTo(value.ToString())}";

                for (int i = 0; i < lines.Count; ++i)
                {
                    string line = lines[i];
                    if (line.TrimStart('#').StartsWith(name))
                    {
                        foundExistingSetting = true;
                        lines[i] = lineToAdd;
                        break;
                    }
                }

                if (!foundExistingSetting)
                {
                    lines.Add(lineToAdd);
                }

                File.WriteAllLines(Values.SshdConfigFile, lines);
            }
        }

        /// <summary>
        /// Transform from SSH syntax to C#
        /// </summary>
        /// <param name="sshSettingValue"></param>
        /// <returns></returns>
        private static string TransformFrom(string sshSettingValue) => sshSettingValue switch
        {
            "no" => false.ToString(),
            "yes" => true.ToString(),
            _ => sshSettingValue
        };

        /// <summary>
        /// Transform from C# to SSH syntax
        /// </summary>
        /// <param name="settingValue"></param>
        /// <returns></returns>
        private static string TransformTo(string settingValue) => settingValue switch
        {
            "False" => "no",
            "True" => "yes",
            _ => settingValue
        };
    }
}
