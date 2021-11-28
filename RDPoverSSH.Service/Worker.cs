using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using PeterKottas.DotNetCore.WindowsService.Interfaces;
using RDPoverSSH.Common;
using RDPoverSSH.DataStore;
using RDPoverSSH.Models;

namespace RDPoverSSH.Service
{
    public class Worker : IMicroService
    {
        #region IMicroService members

        private readonly IMicroServiceController _controller;

        public Worker()
        {
        }

        public Worker(IMicroServiceController controller)
        {
            _controller = controller;
        }

        public void Start()
        {
            _workerThread = new Thread(WorkerThread);
            _workerThread.Start();
        }

        public void Stop()
        {
            // Stop can be called before start, so make it safe.
            _cancellationToken?.Cancel();
            _workerThread?.Join();
            DatabaseEngine.Shutdown();
        }

        #endregion

        #region Private methods

        private async void WorkerThread()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    DoSshServerPoll();
                    DoSshClientPoll();
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry($"RDPoverSSH encountered an error: {ex}", EventLogEntryType.Error);
                }

                // As opposed to Task.Delay, this doesn't throw an exception when the cancellation is set
                await Task.Run(() => Thread.Sleep(TimeSpan.FromSeconds(5)), _cancellationToken.Token);
            }
        }

        private void DoSshServerPoll()
        {
            if (DatabaseEngine.ConnectionCollection.Count(c => c.TunnelDirection == Direction.Incoming) > 0)
            {
                // If we have at least one connection whose tunnel is incoming, we need to make sure ssh server is configured.

                // First, make sure the server is running.
                _sshServiceController.Refresh();
                if (_sshServiceController.Status != ServiceControllerStatus.Running)
                {
                    _sshServiceController.Start();
                }

                // Make sure we have the keys file.
                if (!File.Exists(Values.AdministratorsAuthorizedKeysFilePath))
                {
                    // Create the file
#pragma warning disable CS0642
                    using (File.Create(Values.AdministratorsAuthorizedKeysFilePath));
#pragma warning restore CS0642
                    SetSshAcl(new FileInfo(Values.AdministratorsAuthorizedKeysFilePath));
                    EventLog.WriteEntry($"Created keys file '{Values.AdministratorsAuthorizedKeysFilePath}', and set secure ACLs.");
                }

                string publicKey = null;

                // Now check if our specific key files exist
                if (!File.Exists(Values.OurPrivateKeyFilePath))
                {
#pragma warning disable CS0642
                    using (File.Create(Values.OurPrivateKeyFilePath));
#pragma warning restore CS0642
                    SetSshAcl(new FileInfo(Values.OurPrivateKeyFilePath));
                    EventLog.WriteEntry($"Created keys file '{Values.OurPrivateKeyFilePath}', and set secure ACLs.");

#pragma warning disable CS0642
                    using (File.Create(Values.OurPublicKeyFilePath)) ;
#pragma warning restore CS0642
                    SetSshAcl(new FileInfo(Values.OurPublicKeyFilePath));
                    EventLog.WriteEntry($"Created keys file '{Values.OurPublicKeyFilePath}', and set secure ACLs.");

                    // Now generate our keys
                    using var keygen = new SshKeyGenerator.SshKeyGenerator(2048);

                    File.WriteAllText(Values.OurPrivateKeyFilePath, keygen.ToPrivateKey());
                    File.WriteAllText(Values.OurPublicKeyFilePath, publicKey = keygen.ToRfcPublicKey());

                    EventLog.WriteEntry("Generated RSA public/private keys.");
                }

                if (string.IsNullOrEmpty(publicKey))
                {
                    // This handles the case where we previously created the key, but we still need to add it to the authorized list.
                    publicKey = File.ReadAllText(Values.OurPublicKeyFilePath);
                }

                // Finally, put our private key in the authorized keys file
                if (!File.ReadAllText(Values.AdministratorsAuthorizedKeysFilePath).Contains(publicKey))
                {
                    File.AppendAllText(Values.AdministratorsAuthorizedKeysFilePath, $"{publicKey}\n");
                    EventLog.WriteEntry("Added public key to authorized keys.");
                }
            }
        }

        private void DoSshClientPoll()
        {
            if (DatabaseEngine.ConnectionCollection.Count(c => c.TunnelDirection == Direction.Outgoing) > 0)
            {
                // First make sure we have keys
            }
        }

        /// <summary>
        /// Sets secure ACLs on the given file, according the SSH spec
        /// </summary>
        private void SetSshAcl(FileInfo fileInfo)
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

        private readonly ServiceController _sshServiceController = new ServiceController("sshd");

        #endregion

        #region Threading

        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private Thread _workerThread;

        #endregion

        #region Public static

        public static EventLog EventLog = new EventLog("Application") {Source = "RDPoverSSH"};

        #endregion
    }
}
