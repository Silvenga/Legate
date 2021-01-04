using System;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Legate.Core;
using Legate.Core.Models;
using Legate.Core.State;
using Microsoft.Extensions.Hosting;
using NLog;

namespace Legate.Workers
{
    public class PodWatchingWorker : BackgroundService, IWorker
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IKubernetes _client;
        private readonly IPodsEventStream _podsEventStream;

        public PodWatchingWorker(IKubernetes client, IPodsEventStream podsEventStream)
        {
            _client = client;
            _podsEventStream = podsEventStream;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Watch(cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        Logger.Info("The watcher was unexpectedly closed, will retry.");
                        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    }
                }
                catch (TaskCanceledException)
                {
                    // ignored
                }
                catch (Exception e)
                {
                    Logger.Warn(e, "An exception occurred while watching for pod changes, will retry.");
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }
        }

        private async Task Watch(CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource();

            // TODO Are label selectors case-sensitive?
            var namespaceWatchResponse = await _client.ListPodForAllNamespacesWithHttpMessagesAsync(
                watch: true,
                cancellationToken: cancellationToken,
                labelSelector: $"{KubernetesConstants.TrackedLabel}=true"
            );
            try
            {
                using var watcher = namespaceWatchResponse.Watch<V1Pod, V1PodList>(
                    OnEvent,
                    exception => completionSource.TrySetException(exception),
                    () => completionSource.TrySetResult()
                );

                Logger.Debug("Pod subscriptions created, waiting for events.");

                await Task.WhenAny(
                    Task.Delay(Timeout.Infinite, cancellationToken),
                    completionSource.Task
                );
            }
            finally
            {
                namespaceWatchResponse.Dispose();
            }
        }

        private async void OnEvent(WatchEventType type, V1Pod pod)
        {
            Logger.Debug($"Recieved event '{type}' for pod '{pod.Metadata.Name}'.");

            if (type != WatchEventType.Bookmark)
            {
                await _podsEventStream.WriteEventAsync(new KubernetesUpdate<V1Pod>(type, pod));
            }
        }

        public override void Dispose()
        {
            _podsEventStream.Dispose();
            _client.Dispose();
            base.Dispose();
        }
    }
}