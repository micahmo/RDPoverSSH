using System;
using System.IO;
using System.Linq;
using RDPoverSSH.Common;

namespace RDPoverSSH.Utilities
{
    /// <summary>
    /// A set of utilities related to Rdp
    /// </summary>
    public static class RdpUtils
    {
        /// <summary>
        /// The path to the RDP executable
        /// </summary>
        public static readonly string MstscExecutable = Environment.ExpandEnvironmentVariables(Path.Combine(Environment.SystemDirectory, "mstsc.exe"));

        /// <summary>
        /// The separator character used in RDP Profile names
        /// </summary>
        public static readonly string RdpProfileNameSeparator = "-";

        /// <summary>
        /// Generate a new RDP Profile path using the given connection id and name
        /// </summary>
        public static string NewRdpConnectionFilePath(int connectionId, string name) => Path.Combine(Values.ApplicationDataPath, $"{connectionId}-{name}-{Guid.NewGuid()}.rdp");
        
        /// <summary>
        /// Generate a wild card name that matches every RDP Profile name for the given connection id
        /// </summary>
        public static string RdpConnectionFilesWildcard(int connectionId) => $"{connectionId}-*.rdp";
        
        /// <summary>
        /// Get the user-supplied RDP Profile name from the file name
        /// </summary>
        public static string GetRdpProfileNameFromFileName(string filename) =>
            filename.Split(new[] { RdpProfileNameSeparator }, StringSplitOptions.None).Skip(1).FirstOrDefault();

        /// <summary>
        /// Change the user-supplied portion of the RDP Profile name, given the old file name and the new user-supplied portion
        /// </summary>
        public static string ChangeRdpProfileName(string oldFullName, string newName)
        {
            var parts = oldFullName.Split(new[] { RdpProfileNameSeparator }, StringSplitOptions.None);

            if (parts.Skip(1).Any())
            {
                parts[1] = newName;
            }

            return string.Join(RdpProfileNameSeparator, parts);
        }

        /// <summary>
        /// Fixes up a given RDP Profile file to ensure that the destination host is the given host in the format "name:port" (name is usually localhost)
        /// </summary>
        public static void FixupRdpProfileHost(string filename, string host)
        {
            var profileText = File.ReadAllLines(filename).ToList();
            bool foundAddress = false;
            string newHost = $"full address:s:{host}";

            foreach (int i in Enumerable.Range(0, profileText.Count))
            {
                if (profileText[i].StartsWith("full address"))
                {
                    profileText[i] = newHost;
                    foundAddress = true;
                }
            }

            if (!foundAddress)
            {
                // Somehow, this profile file didn't have any address. Add it now.
                profileText.Add(newHost);
            }

            File.WriteAllLines(filename, profileText);
        }

        /// <summary>
        /// Removes both the reserved <see cref="RdpProfileNameSeparator"/> and illegal Windows filename characters from the given filename
        /// </summary>
        /// <returns></returns>
        public static string SanitizeFilename(string filename)
        {
            filename = filename.Replace(RdpProfileNameSeparator, string.Empty);
            Path.GetInvalidFileNameChars().ToList().ForEach(c => filename = filename.Replace(c.ToString(), string.Empty));

            return filename;
        }
    }
}
