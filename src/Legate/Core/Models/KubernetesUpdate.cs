using k8s;

namespace Legate.Core.Models
{
    public record KubernetesUpdate<T>(WatchEventType Type, T Data);
}