using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using PeterKottas.DotNetCore.WindowsService.Interfaces;
using RDPoverSSH.Common;
using RDPoverSSH.DataStore;
using RDPoverSSH.Models;
using Renci.SshNet;

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
                    DoClean();
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
            if (DatabaseEngine.GetCollection<ConnectionModel>().Count(c => c.TunnelDirection == Direction.Incoming) > 0)
            {
                // If we have at least one connection whose tunnel is incoming, we need to make sure ssh server is configured.

                // First, make sure the server is running.
                _sshServiceController.Refresh();
                if (_sshServiceController.Status != ServiceControllerStatus.Running)
                {
                    _sshServiceController.Start();
                }

                // Make sure we have the keys file.
                if (FileUtils.CreateFileWithSecureAcl(Values.AdministratorsAuthorizedKeysFilePath))
                {
                    EventLog.WriteEntry($"Created keys file '{Values.AdministratorsAuthorizedKeysFilePath}', and set secure ACLs.");
                }

                string publicKey = null;

                // Now check if our specific key files exist
                if (FileUtils.CreateFileWithSecureAcl(Values.OurPrivateKeyFilePath) || FileUtils.CreateFileWithSecureAcl(Values.OurPublicKeyFilePath))
                {
                    EventLog.WriteEntry($"Created keys file '{Values.OurPrivateKeyFilePath}', and set secure ACLs.");
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
            foreach (var connectionModel in DatabaseEngine.GetCollection<ConnectionModel>().Find(c => c.TunnelDirection == Direction.Outgoing).ToList())
            {
                TunnelStatus status;
                string lastError;

                // Check if we have keys
                if (File.Exists(Values.ClientServerPrivateKeyFilePath(connectionModel.ObjectId)))
                {
                    try
                    {
                        var connectionInfo = new ConnectionInfo(connectionModel.TunnelEndpoint, connectionModel.TunnelPort, connectionModel.Username,
                            new PrivateKeyAuthenticationMethod(connectionModel.Username, new PrivateKeyFile(Values.ClientServerPrivateKeyFilePath(connectionModel.ObjectId))));

                        using SftpClient client = new SftpClient(connectionInfo);
                        
                        client.Connect();

                        lastError = string.Empty;
                        status = TunnelStatus.Connected;
                    }
                    catch (Exception ex)
                    {
                        // Log
                        EventLog.WriteEntry($"There was an error creating the SSH client for connection {connectionModel.ObjectId}: {ex}", EventLogEntryType.Warning);

                        lastError = ex.Message;
                        status = TunnelStatus.Disconnected;
                    }
                }
                else
                {
                    lastError = "The server private key is missing.";
                    status = TunnelStatus.Disconnected;
                }

                // Update only the status column
                DatabaseEngine.GetCollection<ConnectionServiceModel>().Upsert(new ConnectionServiceModel
                {
                    ObjectId = connectionModel.ObjectId,
                    Status = status,
                    LastError = lastError
                });
            }
        }

        private void DoClean()
        {
            // Clean any ConnectionServiceModels that refer to non-existent ConnectionModels
            var connectionServiceModelIds = DatabaseEngine.GetCollection<ConnectionServiceModel>().Query().Select(c => c.ObjectId).ToList();
            var connectionModelIds = DatabaseEngine.GetCollection<ConnectionModel>().Query().Select(c => c.ObjectId).ToList();
            var orphanedServiceModelIds = connectionServiceModelIds.Where(c => !connectionModelIds.Contains(c)).ToList();
            orphanedServiceModelIds.ForEach(c => DatabaseEngine.GetCollection<ConnectionServiceModel>().Delete(c));
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
