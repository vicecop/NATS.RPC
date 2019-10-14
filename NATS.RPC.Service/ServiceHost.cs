using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NATS.RPC.Service
{
    public class ServiceHost : BackgroundService
    {
        private readonly IEnumerable<Service> _services;

        public ServiceHost(IEnumerable<Service> services)
        {
            _services = services;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var service in _services)
                service.Start();

            stoppingToken.Register(() =>
            {
                foreach (var service in _services)
                    service.Stop();
            });

            return Task.CompletedTask;
        }
    }
}
