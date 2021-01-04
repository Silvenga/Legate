using Legate.Core.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Legate.Core
{
    public static class LegateExtensions
    {
        public static IHostBuilder SetupLogging(this IHostBuilder builder)
        {
            return builder.ConfigureLogging(logging =>
                          {
                              logging.ClearProviders();
                              logging.SetMinimumLevel(LogLevel.Trace);
                          })
                          .UseNLog();
        }

        public static string GetServiceId(this PodService podService)
        {
            return $"{podService.Name}_{podService.PodUid}";
        }
    }
}