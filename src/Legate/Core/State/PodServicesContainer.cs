using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Legate.Core.Models;
using NLog;

namespace Legate.Core.State
{
    public interface IPodServicesContainer
    {
        Task UpsertPodServicesAsync(Pod pod, IEnumerable<PodService> podServices, CancellationToken cancellationToken = default);
        Task RemovePodServicesAsync(Pod pod, CancellationToken cancellationToken = default);
        IAsyncEnumerable<PodService> GetServicesAsync(CancellationToken cancellationToken = default);
    }

    public class PodServicesContainer : IPodServicesContainer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, IReadOnlyCollection<PodService>> _pods = new();
        private readonly SemaphoreSlim _lock = new(1, 1);

        public async Task UpsertPodServicesAsync(Pod pod, IEnumerable<PodService> podServices, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                _pods[pod.Uid] = podServices.ToList();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task RemovePodServicesAsync(Pod pod, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                _pods.Remove(pod.Uid);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async IAsyncEnumerable<PodService> GetServicesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                foreach (var podService in _pods.SelectMany(podServices => podServices.Value).TakeWhile(_ => !cancellationToken.IsCancellationRequested))
                {
                    yield return podService;
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}