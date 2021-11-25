using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using RDPoverSSH.DataStore;

namespace RDPoverSSH.BusinessLogic
{
    public class SshServerWorker : IWorker
    {
        #region IWorker members

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static SshServerWorker Instance { get; } = new SshServerWorker();

        public void Start()
        {
            _workerThread = new Thread(WorkerThread);
            _workerThread.Start();
        }

        public void Stop()
        {
            _cancellationToken.Cancel();
        }

        #endregion

        #region Private methods

        private async void WorkerThread()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                if (DatabaseEngine.ConnectionCollection.Count(c => c.TunnelDirection == Models.Direction.Incoming) > 0)
                {
                    // If we have at least one connection whose tunnel is incoming, we need to make sure ssh server is running.
                    _sshServiceController.Refresh();

                    if (_sshServiceController.Status != ServiceControllerStatus.Running)
                    {
                        try
                        {
                            _sshServiceController.Start();
                        }
                        catch
                        {
                            // Swallow exceptions, we'll try again shortly
                        }
                    }
                }

                // As opposed to Task.Delay, this doesn't throw an exception when the cancellation is set
                await Task.Run(() => Thread.Sleep(TimeSpan.FromSeconds(5)), _cancellationToken.Token);
            }
        }

        #endregion

        #region Private fields

        private Thread _workerThread;
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        readonly ServiceController _sshServiceController = new ServiceController { ServiceName = "sshd" };

        #endregion
    }
}
