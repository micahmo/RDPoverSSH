using System;
using System.Diagnostics;
using System.Windows;
using Bluegrams.Application;

namespace RDPoverSSH.Utilities
{
    /// <inheritdoc/>
    public class MyUpdateChecker : WpfUpdateChecker
    {
        public MyUpdateChecker(string url, Window owner = null, string identifier = null) : base(url, owner, identifier)
        {
        }

        public override void ShowUpdateDownload(string file)
        {
            // Start the installer
            Process.Start(file);

            // Stop ourselves
            Environment.Exit(0);
        }
    }
}
