using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace Legate.Core
{
    public class LegateConfiguration
    {
        private readonly IConfiguration _configuration;

        public LegateConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string? ConsulToken => _configuration["Consul:Token"];

        [Required, ValidUri(AllowedSchemas = new[] { "http", "https" })]
        public string ConsulHost => _configuration["Consul:Host"] ?? "http://localhost:8500";

        public string? ConsulDatacenter => _configuration["Consul:Datacenter"];

        public int ConsulServiceTtlSeconds => _configuration.GetValue<int?>("Consul:ServiceTtlSeconds") ?? 30;

        public bool Validate(out List<ValidationResult> results)
        {
            var context = new ValidationContext(this, null, null);
            results = new List<ValidationResult>();

            var result = Validator.TryValidateObject(this, context, results, true);
            return result;
        }
    }
}