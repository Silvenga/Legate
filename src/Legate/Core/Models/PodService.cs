using System.Collections.Generic;

namespace Legate.Core.Models
{
    public record PodService(string PodUuid, string ServiceId, int Port, IReadOnlyCollection<string> Tags, PodServiceScope Scope);
}