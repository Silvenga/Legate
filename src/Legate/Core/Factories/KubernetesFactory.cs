using JetBrains.Annotations;
using k8s;
using Microsoft.Extensions.Hosting;

namespace Legate.Core.Factories
{
    public interface IKubernetesFactory
    {
        KubernetesClientConfiguration CreateConfiguration();
        IKubernetes CreateClient();
    }

    [UsedImplicitly]
    public class KubernetesFactory : IKubernetesFactory
    {
        private readonly IHostEnvironment _environment;

        public KubernetesFactory(IHostEnvironment environment)
        {
            _environment = environment;
        }

        public KubernetesClientConfiguration CreateConfiguration()
        {
            if (_environment.IsDevelopment())
            {
                return new KubernetesClientConfiguration
                {
                    Host = "http://127.0.0.1:8001"
                };
            }

            return KubernetesClientConfiguration.InClusterConfig();
        }

        public IKubernetes CreateClient()
        {
            return new Kubernetes(CreateConfiguration());
        }
    }
}