using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Legate.Core.Models;

namespace Legate.Core.Clients
{
    public interface IConsulServiceClient : IDisposable
    {
        Task<string> RegisterPodServiceAsync(PodService podService, TimeSpan ttl, CancellationToken cancellationToken = default);
        Task<string> RemovePodService(PodService podService, CancellationToken cancellationToken = default);
        Task MaintainPodService(PodService podService, string status, CancellationToken cancellationToken = default);
    }

    public class ConsulServiceClient : IConsulServiceClient
    {
        private readonly ConsulClient _client;

        public ConsulServiceClient(ConsulClient client)
        {
            _client = client;
        }

        public async Task<string> RegisterPodServiceAsync(PodService podService, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            var serviceId = podService.GetServiceId();
            var service = new AgentServiceRegistration
            {
                ID = podService.Name + "_" + podService.PodUid,
                Meta = new Dictionary<string, string>
                {
                    { "Legate-PodUid", podService.PodUid },
                    { "Legate-Scope", podService.Scope.ToString() },
                    { "Legate-IsManaged", true.ToString() }
                },
                Tags = podService.Tags.ToArray(),
                Port = podService.Port,
                Name = podService.Name,
                Check = new AgentServiceCheck
                {
                    ID = serviceId,
                    TTL = ttl,
                    DeregisterCriticalServiceAfter = ttl * 2
                }
            };
            await _client.Agent.ServiceRegister(service, cancellationToken);
            return serviceId;
        }

        public async Task<string> RemovePodService(PodService podService, CancellationToken cancellationToken = default)
        {
            var serviceId = podService.GetServiceId();
            await _client.Agent.ServiceDeregister(serviceId, cancellationToken);
            return serviceId;
        }

        public async Task MaintainPodService(PodService podService, string status, CancellationToken cancellationToken = default)
        {
            await _client.Agent.UpdateTTL(podService.GetServiceId(), status, TTLStatus.Pass, cancellationToken);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}