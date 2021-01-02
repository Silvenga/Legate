using System.Collections.Generic;
using System.Linq;

namespace Legate.Core.Models
{
    public class ParseErrors
    {
        private readonly HashSet<string> _errors = new();

        public int Count => _errors.Count;

        public void Add(string errorMessage)
        {
            _errors.Add(errorMessage);
        }

        public IReadOnlyCollection<string> GetErrorMessages()
        {
            return _errors.ToList();
        }
    }
}