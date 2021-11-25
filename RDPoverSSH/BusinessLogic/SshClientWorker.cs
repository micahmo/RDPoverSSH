namespace RDPoverSSH.BusinessLogic
{
    public class SshClientWorker : IWorker
    {
        #region IWorker members

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static SshClientWorker Instance { get; } = new SshClientWorker();


        public void Start()
        {
            
        }

        public void Stop()
        {
            
        }

        #endregion
    }
}
