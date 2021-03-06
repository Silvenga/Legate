﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NLog;

namespace Legate.Workers
{
    public class LegateWorker : BackgroundService, IWorker
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Logger.Info("Starting core workers.");
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            finally
            {
                Logger.Info("Shutdown request recieved.");
            }
        }
    }
}