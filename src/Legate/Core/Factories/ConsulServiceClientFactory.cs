using System;
using Consul;

namespace Legate.Core.Factories
{
    public interface IConsulServiceClientFactory
    {
        IConsulClient Create();
    }

    public class ConsulServiceClientFactory : IConsulServiceClientFactory
    {
        private readonly LegateConfiguration _configuration;

        public ConsulServiceClientFactory(LegateConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConsulClient Create()
        {
            return new ConsulClient(client =>
            {
                client.Token = _configuration.ConsulToken;
                client.Address = new Uri(_configuration.ConsulHost);
                client.Datacenter = _configuration.ConsulDatacenter;
            });
        }
    }
}