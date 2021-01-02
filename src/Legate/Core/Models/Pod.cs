using System.Collections.Generic;

namespace Legate.Core.Models
{
    public record Pod(string Uid,
                      string Namespace,
                      string Name,
                      string Host,
                      string PodAddress,
                      bool Ready,
                      IReadOnlyCollection<PodPort> PodPorts,
                      IReadOnlyDictionary<string, string> PodLabels,
                      IReadOnlyDictionary<string, string> PodAnnotations);

    public record PodPort(string ContainerName, string? Name, int Port);
}