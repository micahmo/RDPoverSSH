using System.Diagnostics;
using PeterKottas.DotNetCore.WindowsService;

namespace RDPoverSSH.Service
{
    class Program
    {
        public static void Main(string[] args)
        {
            ServiceRunner<Worker>.Run(config =>
            {
                config.SetName("RDPoverSSH.Service.Worker");
                config.SetDisplayName("RDPoverSSH Worker Service");
                config.SetDescription("Manages OpenSSH Server and Client for RDPoverSSH.");

                config.Service(serviceConfig =>
                {
                    serviceConfig.ServiceFactory((extraArguments, controller) => new Worker(controller));

                    serviceConfig.OnStart((service, extraParams) =>
                    {
                        service.Start();
                        Worker.EventLog.WriteEntry("RDPoverSSH.Service started gracefully.", EventLogEntryType.Information);
                    });

                    serviceConfig.OnStop(service =>
                    {
                        service.Stop();
                        Worker.EventLog.WriteEntry("RDPoverSSH.Service stopped gracefully.", EventLogEntryType.Information);
                    });

                    serviceConfig.OnError(e =>
                    {
                        Worker.EventLog.WriteEntry($"RDPoverSSH.Service encountered an error: {e}", EventLogEntryType.Error);
                    });
                });
            });
        }
    }
}