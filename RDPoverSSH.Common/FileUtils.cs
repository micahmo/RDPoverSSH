using System.IO;
using System.Security.AccessControl;

namespace RDPoverSSH.Common
{
    /// <summary>
    /// A set of utilities related to file management
    /// </summary>
    public static class FileUtils
    {
        #region Public methods

        /// <summary>
        /// If a file with the given <paramref name="fullPath"/> does not yet exist,
        /// creates it, and adds the secure (SSH recommended) ACLs.
        /// If the file exists, no action is taken.
        /// Returns true if the file was created, otherwise false.
        /// </summary>
        public static bool CreateFileWithSecureAcl(string fullPath)
        {
            // Make sure we have the keys file.
            if (!File.Exists(fullPath))
            {
                // Create the file
#pragma warning disable CS0642
                using (File.Create(fullPath));
#pragma warning restore CS0642
                SetSshAcl(new FileInfo(fullPath));

                return true;
            }

            return false;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Sets secure ACLs on the given file, according the SSH spec
        /// </summary>
        private static void SetSshAcl(FileInfo fileInfo)
        {
            // Have to set the ACLs for security and for SSH to be happy.
            // This follow the steps exactly from here: https://stackoverflow.com/a/64868357/4206279
            // Setting ACL seems to work better on FileInfo than FileStream
            FileSecurity keysFileAccessControl = fileInfo.GetAccessControl();
            keysFileAccessControl.SetAccessRuleProtection(true, false);
            keysFileAccessControl.SetAccessRule(new FileSystemAccessRule("Administrators", FileSystemRights.FullControl, AccessControlType.Allow));
            keysFileAccessControl.SetAccessRule(new FileSystemAccessRule("SYSTEM", FileSystemRights.FullControl, AccessControlType.Allow));
            fileInfo.SetAccessControl(keysFileAccessControl);
        }

        #endregion
    }
}
