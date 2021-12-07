using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Win32;
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

                if (_sshServiceController.Status == ServiceControllerStatus.Running)
                {
                    if (connectionModel.IsReverseTunnel)
                    {
                        // For a reverse tunnel, we can check if we have a connection
                        try
                        {
                            var res = (Cli.Wrap("netstat").WithArguments("-ano") | Cli.Wrap("findstr").WithArguments($"\"\\[::1\\]:{connectionModel.LocalTunnelPort}\""))
                                .WithValidation(CommandResultValidation.None)
                                .ExecuteBufferedAsync().GetAwaiter().GetResult()
                                .StandardOutput;

                            if (!string.IsNullOrEmpty(res) // See if we got any output from netstat after filtering through findstr.
                                && int.TryParse(res.Split().LastOrDefault(s => !string.IsNullOrEmpty(s)), out int pid) // Parse the netstat output; the last non-empty column is the PID
                                && Process.GetProcessById(pid).ProcessName.Equals(_sshServiceName)) // See if the PID holding this port is sshd
                            {
                                // We found the SSH server process listening on our LocalTunnelPort, so we're totally connected.
                                connectionServiceModel.Status = TunnelStatus.Connected;
                            }
                            else
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
                    Direction = Direction.Outgoing
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
                    // Check if we have keys
                    if (File.Exists(Values.ClientServerPrivateKeyFilePath(connectionModel.ObjectId)))
                    {
                        try
                        {
                            var connectionInfo = new ConnectionInfo(connectionModel.TunnelEndpoint, connectionModel.TunnelPort, Values.RdpOverSshWindowsUsername,
                                new PrivateKeyAuthenticationMethod(Values.RdpOverSshWindowsUsername, new PrivateKeyFile(Values.ClientServerPrivateKeyFilePath(connectionModel.ObjectId))))
                            {
                                Timeout = TimeSpan.FromSeconds(10)
                            };

                            SshClient client = new SshClient(connectionInfo);
                            client.Connect();

                            connectionServiceModel.LastError = string.Empty;
                            connectionServiceModel.Status = TunnelStatus.Connected;

                            // We're connected, now make a long-running connection to keep open the tunnel
                            ForwardedPort forwardedPort = connectionModel.IsReverseTunnel switch
                            {
                                true => new ForwardedPortRemote("localhost", (uint)connectionModel.LocalTunnelPort, string.Empty, (uint)connectionModel.ConnectionPort),
                                false => new ForwardedPortLocal("localhost", (uint)connectionModel.LocalTunnelPort, string.Empty, (uint)connectionModel.ConnectionPort)
                            };

                            client.AddForwardedPort(forwardedPort);

                            forwardedPort.Exception += (_, args) =>
                            {
                                EventLog.WriteEntry($"There was an exception with the from forwarded port for connection \"{connectionModel}\": {args.Exception}", EventLogEntryType.Warning);

                                // A port exception essentially kills the tunnel, so dispose and remove it now.
                                DeleteClient(connectionModel.ObjectId);
                            };

                            forwardedPort.Start();

                            _sshClients[connectionModel.ObjectId] = client;
                        }
                        catch (Exception ex)
                        {
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
                kvp.Value?.Dispose();
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

        #endregion

        #region Private fields

        private readonly ServiceController _sshServiceController = new ServiceController(_sshServiceName);
        private readonly Dictionary<int, SshClient> _sshClients = new Dictionary<int, SshClient>();

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
