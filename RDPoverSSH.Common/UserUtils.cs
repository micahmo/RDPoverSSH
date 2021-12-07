using System;
using System.DirectoryServices;
using System.Linq;
using Microsoft.Win32;

namespace RDPoverSSH.Common
{
    /// <summary>
    /// A set of utilities related to working with Windows users
    /// </summary>
    public static class UserUtils
    {
        /// <summary>
        /// Does the given Windows user exist (case-sensitive)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool WindowsUserExists(string name)
        {
            DirectoryEntry ad = new DirectoryEntry($"WinNT://{Environment.MachineName},computer");
            return ad.Children.OfType<DirectoryEntry>().FirstOrDefault(d => d.Properties["Name"].Value as string == name) != null;
        }

        /// <summary>
        /// Create the given Windows user as a hidden administrator
        /// </summary>
        public static void CreateWindowsUser(string name)
        {
            DirectoryEntry ad = new DirectoryEntry($"WinNT://{Environment.MachineName},computer");
            DirectoryEntry user = ad.Children.Add(name, "user");
            user.CommitChanges();

            DirectoryEntry administratorsGroup = ad.Children.Find("Administrators", "group");
            administratorsGroup.Invoke("Add", user.Path);

            // Add a special registry entry that causes this user to be hidden from most of the user-facing user management
            var specialAccounts = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\SpecialAccounts\UserList");
            specialAccounts?.SetValue(name, 0);
        }

        /// <summary>
        /// Delete the given Windows user
        /// </summary>
        public static void DeleteWindowsUser(string name)
        {
            DirectoryEntry ad = new DirectoryEntry($"WinNT://{Environment.MachineName},computer");

            if (ad.Children.OfType<DirectoryEntry>().FirstOrDefault(d => d.Properties["Name"].Value as string == name) is DirectoryEntry user)
            {
                ad.Children.Remove(user);
            }

            var specialAccounts = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\SpecialAccounts\UserList", true);
            if (specialAccounts?.GetValue(name) != null)
            {
                specialAccounts.DeleteValue(name);
            }
        }
    }
}
