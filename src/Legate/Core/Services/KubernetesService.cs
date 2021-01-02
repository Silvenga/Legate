using System;
using System.Threading.Tasks;
using k8s;
using k8s.Models;

namespace Legate.Core.Services
{
    public class KubernetesService : IDisposable
    {
        private readonly IKubernetes _client;

        public KubernetesService(IKubernetes client)
        {
            _client = client;
        }

        public async Task METHOD_NAME()
        {
            var namespaceWatchResponse = await _client.ListPodForAllNamespacesWithHttpMessagesAsync(watch: true);

            namespaceWatchResponse.Watch<V1Pod, V1PodList>((type, pod) =>
            {

            });
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}