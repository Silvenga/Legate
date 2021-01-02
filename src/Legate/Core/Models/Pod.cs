namespace Legate.Core.Models
{
    public class Pod
    {
        public string Namespace { get; set; }

        public string Name { get; set; }

        public string Host { get; set; }

        public Pod(string ns, string name, string host)
        {
            Namespace = ns;
            Name = name;
            Host = host;
        }
    }
}