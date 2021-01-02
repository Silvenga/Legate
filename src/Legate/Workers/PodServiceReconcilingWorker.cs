using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Legate.Core;
using Legate.Core.Models;
using Legate.Core.State;
using Microsoft.Extensions.Hosting;
using NLog;
using Org.BouncyCastle.Asn1;

namespace Legate.Workers
{
    public class PodServiceReconcilingWorker : BackgroundService, IWorker
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IPodServicesContainer _podServicesContainer;
        private readonly IPodsEventStream _podsEventStream;
        private readonly IServiceAnnotationMapper _serviceAnnotationMapper;

        public PodServiceReconcilingWorker(IPodServicesContainer podServicesContainer, IPodsEventStream podsEventStream,
                                           IServiceAnnotationMapper serviceAnnotationMapper)
        {
            _podServicesContainer = podServicesContainer;
            _podsEventStream = podsEventStream;
            _serviceAnnotationMapper = serviceAnnotationMapper;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await foreach (var (updateType, data) in _podsEventStream.ReadEventsAsync(cancellationToken))
            {
                var pod = GetPod(data);
                switch (updateType)
                {
                    case WatchEventType.Added:
                    case WatchEventType.Modified:
                        var services = GetPodServices(pod);
                        await _podServicesContainer.UpsertPodServicesAsync(pod, services, cancellationToken);
                        break;
                    case WatchEventType.Deleted:
                    case WatchEventType.Error:
                        await _podServicesContainer.RemovePodServicesAsync(pod, cancellationToken);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static Pod GetPod(V1Pod data)
        {
            var podNamespace = string.IsNullOrWhiteSpace(data.Metadata.NamespaceProperty)
                ? "default"
                : data.Metadata.NamespaceProperty;

            var podPorts = data.Spec.Containers
                               .SelectMany(x => x.Ports.Select(c => new
                               {
                                   Container = x,
                                   Port = c
                               }))
                               .Select(x => new PodPort(x.Container.Name, x.Port.Name, x.Port.ContainerPort))
                               .ToList();

            var pod = new Pod(
                data.Metadata.Uid,
                podNamespace,
                data.Metadata.Name,
                data.Spec.NodeName,
                data.Status.PodIP,
                podPorts,
                new Dictionary<string, string>(data.Metadata.Labels ?? new Dictionary<string, string>()),
                new Dictionary<string, string>(data.Metadata.Annotations ?? new Dictionary<string, string>())
            );
            return pod;
        }

        private IEnumerable<PodService> GetPodServices(Pod pod)
        {
            var serviceAnnotations = pod.PodAnnotations.Where(x => x.Key.StartsWith(KubernetesConstants.ServicePrefixAnnotation));
            foreach (var annotation in serviceAnnotations)
            {
                if (_serviceAnnotationMapper.TryMap(annotation.Key, annotation.Value, pod.PodPorts, out var parsedPodServiceAnnotation, out var parseErrors))
                {
                    yield return new PodService(
                        pod.Uid,
                        parsedPodServiceAnnotation.ServiceName,
                        parsedPodServiceAnnotation.Port,
                        parsedPodServiceAnnotation.Tags,
                        parsedPodServiceAnnotation.Scope
                    );
                }

                if (parseErrors.Count > 0)
                {
                    var errors = string.Join(", ", parseErrors.GetErrorMessages());
                    Logger.Warn($"Errors occured while parsing the annotation '{annotation.Key}' '{annotation.Value}' on pod '{pod.Name}': {errors}.");
                }
            }
        }
    }
}