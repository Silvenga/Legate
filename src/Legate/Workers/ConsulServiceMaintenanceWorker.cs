using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Legate.Core;
using Legate.Core.Clients;
using Legate.Core.State;
using Microsoft.Extensions.Hosting;
using NLog;

namespace Legate.Workers
{
    public class ConsulServiceMaintenanceWorker : BackgroundService, IWorker
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly LegateConfiguration _configuration;
        private readonly IConsulServiceClient _client;
        private readonly IPodServicesContainer _podServicesContainer;

        public ConsulServiceMaintenanceWorker(LegateConfiguration configuration, IConsulServiceClient client, IPodServicesContainer podServicesContainer)
        {
            _configuration = configuration;
            _client = client;
            _podServicesContainer = podServicesContainer;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var timeout = TimeSpan.FromSeconds(_configuration.ConsulServiceTtlSeconds);
                await Task.Delay(timeout, cancellationToken);

                await foreach (var podService in _podServicesContainer.GetServicesAsync(cancellationToken))
                {
                    var builder = new StringBuilder();
                    builder.Append("Pod: ").Append(podService.PodUid).AppendLine();

                    try
                    {
                        await _client.MaintainPodService(podService, builder.ToString(), cancellationToken);
                    }
                    catch (Exception e)
                    {
                        Logger.Warn(e, $"A handled exception occurred while updating the service check {podService.Name} for pod '{podService.PodUid}'.");
                    }
                }
            }
        }
    }
}