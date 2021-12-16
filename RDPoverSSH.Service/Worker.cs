using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
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
                _skipWait = false;

                try
                {
                    DoSshServerPoll();
                    DoSshClientPoll();
                    DoClean();
                    FixCommonProblems();
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry($"RDPoverSSH encountered an error: {ex}", EventLogEntryType.Error);
                }

                if (!_skipWait)
                {
                    // As opposed to Task.Delay, this doesn't throw an exception when the cancellation is set
                    await Task.Run(() => Thread.Sleep(TimeSpan.FromSeconds(5)), _cancellationToken.Token);
                }
            }
        }

        private void DoSshServerPoll()
        {
            bool sshServerInstalled = true;

            if (DatabaseEngine.GetCollection<ConnectionModel>().Count(c => c.TunnelDirection == Direction.Incoming) > 0)
            {
                // If we have at least one connection whose tunnel is incoming, we need to make sure ssh server is configured.

                // First, make sure the server is running.
                try
                {
                    _sshServiceController.Refresh();
                    if (_sshServiceController.Status != ServiceControllerStatus.Running)
                    {
                        _sshServiceController.Start();
                    }
                }
                catch
                {
                    // Error loading the service. It must not be installed.
                    sshServerInstalled = false;
                }

                // Check if we have our user
                if (!UserUtils.WindowsUserExists(Values.RdpOverSshWindowsUsername))
                {
                    UserUtils.CreateWindowsUser(Values.RdpOverSshWindowsUsername);
                    EventLog.WriteEntry($"Created Windows user '{Values.RdpOverSshWindowsUsername}'.");
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

            // Now update some statues for each incoming connection
            foreach (var connectionModel in DatabaseEngine.GetCollection<ConnectionModel>().Find(c => c.TunnelDirection == Direction.Incoming).ToList())
            {
                // Create a server-side representation of our connection
                ConnectionServiceModel connectionServiceModel = new ConnectionServiceModel
                {
                    ObjectId = connectionModel.ObjectId,
                    Direction = Direction.Incoming
                };

                if (sshServerInstalled)
                {
                    if (_sshServiceController.Status == ServiceControllerStatus.Running)
                    {
                        if (connectionModel.IsReverseTunnel)
                        {
                            // For a reverse tunnel, we can check if we have a connection
                            try
                            {
                                var processes = (Cli.Wrap("netstat").WithArguments("-ano") | Cli.Wrap("findstr").WithArguments($"\"{IPAddress.Loopback}:{connectionModel.LocalTunnelPort}\""))
                                    .WithValidation(CommandResultValidation.None)
                                    .ExecuteBufferedAsync().GetAwaiter().GetResult()
                                    .StandardOutput.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

                                bool foundSshd = false;
                                foreach (string process in processes)
                                {
                                    if (!string.IsNullOrEmpty(process) // See if we got any output from netstat after filtering through findstr.
                                        && int.TryParse(process.Split().LastOrDefault(s => !string.IsNullOrEmpty(s)), out int pid) // Parse the netstat output; the last non-empty column is the PID
                                        && Process.GetProcessById(pid).ProcessName.Equals(_sshServiceName)) // See if the PID holding this port is sshd
                                    {
                                        // We found the SSH server process listening on our LocalTunnelPort, so we're totally connected.
                                        foundSshd = true;
                                        connectionServiceModel.Status = TunnelStatus.Connected;
                                    }
                                }

                                if (!foundSshd)
                                {
                                    // We couldn't find the SSH server process listening on our LocalTunnelPort, so we're not totally connected.
                                    connectionServiceModel.Status = TunnelStatus.Partial;
                                }
                            }
                            catch
                            {
                                // If there's any exception trying to get a process listening on our LocalTunnelPort, we can't prove that it's working.
                                connectionServiceModel.Status = TunnelStatus.Partial;
                            }
                        }
                        else
                        {
                            // Not a reverse tunnel, so all we can do is verify that sshd is running.
                            connectionServiceModel.Status = TunnelStatus.Connected;
                        }
                    }
                    else
                    {
                        // SSH server isn't running.
                        connectionServiceModel.Status = TunnelStatus.Disconnected;
                    }
                }
                else
                {
                    // SSH server isn't installed.
                    connectionServiceModel.Status = TunnelStatus.Uninstalled;
                }

                // Update based on the changes we made
                DatabaseEngine.GetCollection<ConnectionServiceModel>().Upsert(connectionServiceModel);
            }
        }

        private void DoSshClientPoll()
        {
            foreach (var connectionModel in DatabaseEngine.GetCollection<ConnectionModel>().Find(c => c.TunnelDirection == Direction.Outgoing).ToList())
            {
                bool needNewTunnel = false;

                // Create a server-side representation of our connection
                ConnectionServiceModel connectionServiceModel = new ConnectionServiceModel
                {
                    ObjectId = connectionModel.ObjectId,
                    Direction = Direction.Outgoing,
                    RemoteMachineName = DatabaseEngine.GetCollection<ConnectionServiceModel>().FindById(connectionModel.ObjectId)?.RemoteMachineName
                };

                if (_sshClients.TryGetValue(connectionModel.ObjectId, out var existingClient))
                {
                    try
                    {
                        if (AreEqual(existingClient, connectionModel) && existingClient.IsConnected && existingClient.ForwardedPorts.FirstOrDefault()?.IsStarted == true)
                        {
                            connectionServiceModel.LastError = string.Empty;
                            connectionServiceModel.Status = TunnelStatus.Connected;
                        }
                        else
                        {
                            DeleteClient(connectionModel.ObjectId);
                            needNewTunnel = true;
                        }
                    }
                    catch
                    {
                        // There may be some exception, such as the client is already disposed
                        // (Unfortunately, there's no way to check for disposed)
                        DeleteClient(connectionModel.ObjectId);
                        needNewTunnel = true;
                    }
                }
                else
                {
                    needNewTunnel = true;
                }

                if (needNewTunnel)
                {
                    connectionServiceModel.RemoteMachineName = string.Empty;

                    // Check if we have keys
                    if (File.Exists(Values.ClientServerPrivateKeyFilePath(connectionModel.ObjectId)))
                    {
                        try
                        {
                            var connectionInfo = new ConnectionInfo(connectionModel.TunnelEndpoint, connectionModel.TunnelPort, Values.RdpOverSshWindowsUsername,
                                new PrivateKeyAuthenticationMethod(Values.RdpOverSshWindowsUsername, new PrivateKeyFile(Values.ClientServerPrivateKeyFilePath(connectionModel.ObjectId))))
                            {
                                Timeout = TimeSpan.FromSeconds(10),
                            };

                            SshClient client = new SshClient(connectionInfo)
                            {
                                KeepAliveInterval = TimeSpan.FromSeconds(10)
                            };

                            // Track the client immediately after instantiating (before connecting)
                            // So that we can clean it up if anything goes wrong.
                            _sshClients[connectionModel.ObjectId] = client;

                            client.Connect();

                            connectionServiceModel.LastError = string.Empty;
                            connectionServiceModel.Status = TunnelStatus.Connected;

                            // We're connected, now make a long-running connection to keep open the tunnel
                            ForwardedPort forwardedPort = connectionModel.IsReverseTunnel switch
                            {
                                true => new ForwardedPortRemote(IPAddress.Loopback.ToString(), (uint)connectionModel.LocalTunnelPort, IPAddress.Loopback.ToString(), (uint)connectionModel.ConnectionPort),
                                false => new ForwardedPortLocal(IPAddress.Loopback.ToString(), (uint)connectionModel.LocalTunnelPort, IPAddress.Loopback.ToString(), (uint)connectionModel.ConnectionPort)
                            };

                            client.AddForwardedPort(forwardedPort);

                            forwardedPort.Exception += (_, args) =>
                            {
                                EventLog.WriteEntry($"There was an exception with the from forwarded port for connection \"{connectionModel}\": {args.Exception}", EventLogEntryType.Warning);

                                // A port exception essentially kills the tunnel, so dispose and remove it now.
                                DeleteClient(connectionModel.ObjectId);
                            };

                            forwardedPort.Start();

                            // Get the hostname over SSH
                            string hostname = client.CreateCommand("hostname").Execute();
                            connectionServiceModel.RemoteMachineName = hostname;
                        }
                        catch (Exception ex)
                        {
                            // In case we got far enough to create a client but failed later (e.g., while port forwarding) clean up the client
                            DeleteClient(connectionModel.ObjectId);

                            // Log
                            EventLog.WriteEntry($"There was an error creating the SSH client for connection \"{connectionModel}\": {ex}", EventLogEntryType.Warning);

                            connectionServiceModel.LastError = ex.Message;
                            connectionServiceModel.Status = TunnelStatus.Disconnected;
                        }
                    }
                    else
                    {
                        connectionServiceModel.LastError = "The server private key is missing.";
                        connectionServiceModel.Status = TunnelStatus.Disconnected;
                    }
                }

                // Update based on the changes we made
                DatabaseEngine.GetCollection<ConnectionServiceModel>().Upsert(connectionServiceModel);
            }
        }

        private void DoClean()
        {
            // Clean any ConnectionServiceModels that refer to non-existent ConnectionModels
            var connectionServiceModelIds = DatabaseEngine.GetCollection<ConnectionServiceModel>().Query().Select(c => c.ObjectId).ToList();
            var connectionModelIds = DatabaseEngine.GetCollection<ConnectionModel>().Query().Select(c => c.ObjectId).ToList();
            var orphanedServiceModelIds = connectionServiceModelIds.Where(c => !connectionModelIds.Contains(c)).ToList();
            orphanedServiceModelIds.ForEach(c =>
            {
                DeleteClient(c);
                DatabaseEngine.GetCollection<ConnectionServiceModel>().Delete(c);
            });
        }

        private void DeleteClient(int connectionId)
        {
            _sshClients.Where(kvp => kvp.Key == connectionId).Select(kvp => kvp).ToList().ForEach(kvp =>
            {
                try
                {
                    kvp.Value?.Dispose();
                }
                catch
                {
                    // Swallow. SshClient throws a rare "collection modified" exception when disposing ports. 
                    // https://github.com/sshnet/SSH.NET/blob/a5bd08d655bb6a3c762306472cec354556dca3a3/src/Renci.SshNet/SshClient.cs#L167
                }

                _sshClients.Remove(kvp.Key);
            });
        }

        private bool AreEqual(SshClient client, ConnectionModel connection)
        {
            if (client.ConnectionInfo.Host != connection.TunnelEndpoint)
            {
                return false;
            }

            if (client.ConnectionInfo.Port != connection.TunnelPort)
            {
                return false;
            }

            if (connection.IsReverseTunnel)
            {
                if (client.ForwardedPorts.FirstOrDefault() is ForwardedPortRemote forwardedPortRemote)
                {
                    if (forwardedPortRemote.Port != connection.ConnectionPort)
                    {
                        return false;
                    }

                    if (forwardedPortRemote.BoundPort != connection.LocalTunnelPort)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (client.ForwardedPorts.FirstOrDefault() is ForwardedPortLocal forwardedPortLocal)
                {
                    if (forwardedPortLocal.Port != connection.ConnectionPort)
                    {
                        return false;
                    }

                    if (forwardedPortLocal.BoundPort != connection.LocalTunnelPort)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            try
            {
                byte[] existingClientServerPrivateKey = (client.ConnectionInfo.AuthenticationMethods.FirstOrDefault() as PrivateKeyAuthenticationMethod)?.KeyFiles.FirstOrDefault()?.HostKey.Data;
                byte[] currentClientServerPrivateKey = new PrivateKeyFile(Values.ClientServerPrivateKeyFilePath(connection.ObjectId)).HostKey.Data;

                if (!(((IStructuralEquatable)existingClientServerPrivateKey)?.Equals(currentClientServerPrivateKey, StructuralComparisons.StructuralEqualityComparer) ?? false))
                {
                    // The key we were using to connect does not match the key in the current file. Invalidate.
                    return false;
                }
            }
            catch
            {
                // Any exception constructing our current private key (not found, invalid format), means it changed and we need to invalidate.
                return false;
            }

            return true;
        }

        private void FixCommonProblems()
        {
            // Handle incoming tunnels needing fixing -- just restart sshd
            if (DatabaseEngine.GetCollection<ConnectionModel>().Find(c => c.IsFixRequested && c.TunnelDirection == Direction.Incoming).Any())
            {
                // Kill all sshd processes
                foreach (Process sshdProcess in Process.GetProcessesByName(_sshServiceName))
                {
                    sshdProcess.Kill();
                }

                _skipWait = true;
            }

            // Handle outgoing tunnels needing fixing, kill each client
            foreach (var connection in DatabaseEngine.GetCollection<ConnectionModel>().Find(c => c.IsFixRequested && c.TunnelDirection == Direction.Outgoing).ToList())
            {
                DeleteClient(connection.ObjectId);
                _skipWait = true;
            }
        }

        #endregion

        #region Private fields

        private readonly ServiceController _sshServiceController = new ServiceController(_sshServiceName);
        private readonly Dictionary<int, SshClient> _sshClients = new Dictionary<int, SshClient>();
        private bool _skipWait;

        #endregion

        #region Private static

        private static readonly string _sshServiceName = "sshd";

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
