using System.Collections.Generic;

namespace Legate.Core.Models
{
    public record PodServiceAnnotation(string OriginalKey, string ServiceName, int Port, IReadOnlyCollection<string> Tags, PodServiceScope Scope);
}