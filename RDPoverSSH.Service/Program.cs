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

                var name = config.GetDefaultName();
                config.Service(serviceConfig =>
                {
                    serviceConfig.ServiceFactory((extraArguments, controller) => new Worker(controller));

                    serviceConfig.OnStart((service, extraParams) =>
                    {
                        service.Start();
                    });

                    serviceConfig.OnStop(service =>
                    {
                        service.Stop();
                    });

                    serviceConfig.OnError(e =>
                    {
                        // TODO: Add error logging
                    });
                });
            });
        }
    }
}