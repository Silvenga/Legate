using JetBrains.Annotations;
using k8s.Models;
using Legate.Core.Models;

namespace Legate.Core.State
{
    public interface IPodsEventStream : IEventStream<KubernetesUpdate<V1Pod>>
    {
    }

    [UsedImplicitly]
    public class PodsEventStream : BaseEventStream<KubernetesUpdate<V1Pod>>, IPodsEventStream
    {
    }
}