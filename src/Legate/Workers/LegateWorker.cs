using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NLog;

namespace Legate.Workers
{
    public class LegateWorker : BackgroundService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Info("Starting core workers.");
        }
    }
}