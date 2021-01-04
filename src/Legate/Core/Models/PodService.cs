using System.Collections.Generic;

namespace Legate.Core.Models
{
    public record PodService(string PodUid, string Name, int Port, IReadOnlyCollection<string> Tags, PodServiceScope Scope);
}