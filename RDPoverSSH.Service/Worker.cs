using System;
using System.Diagnostics;
using System.ServiceProcess;
using PeterKottas.DotNetCore.WindowsService.Base;
using PeterKottas.DotNetCore.WindowsService.Interfaces;
using RDPoverSSH.DataStore;

namespace RDPoverSSH.Service
{
    public class Worker : MicroService, IMicroService
    {
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
            StartBase();

            Timers.Start("SshServerPoller", (int)TimeSpan.FromSeconds(5).TotalMilliseconds, DoSshServerPoll,
                e =>
                {
                    EventLog.WriteEntry($"RDPoverSSH.Service.SshServerPoller encountered an error: {e}", EventLogEntryType.Error);
                });

            Timers.Start("SshClientPoller", (int)TimeSpan.FromSeconds(5).TotalMilliseconds, () => { },
                e =>
                {
                    EventLog.WriteEntry($"RDPoverSSH.Service.SshClientPoller encountered an error: {e}", EventLogEntryType.Error);
                });
        }

        public void Stop()
        {
            StopBase();
            DatabaseEngine.Shutdown();
        }

        #region Private methods

        private void DoSshServerPoll()
        {
            if (DatabaseEngine.ConnectionCollection.Count(c => c.TunnelDirection == Models.Direction.Incoming) > 0)
            {
                // If we have at least one connection whose tunnel is incoming, we need to make sure ssh server is running.
                _sshServiceController.Refresh();

                if (_sshServiceController.Status != ServiceControllerStatus.Running)
                {
                    _sshServiceController.Start();
                }
            }
        }

        private readonly ServiceController _sshServiceController = new ServiceController("sshd");

        #endregion

        #region Public static

        public static EventLog EventLog = new EventLog("Application") {Source = "RDPoverSSH"};

        #endregion
    }
}
