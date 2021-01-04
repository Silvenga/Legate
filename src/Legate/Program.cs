using System;
using Lamar.Microsoft.DependencyInjection;
using Legate.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Web;

namespace Legate
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // ReSharper disable once StringLiteralTypo
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                Start(args, logger);
            }
            catch (Exception e)
            {
                logger.Error(e, "An unhandled exception occured.");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static void Start(string[] args, ILogger logger)
        {
            var host = CreateHostBuilder(args).Build();
            var configuration = host.Services.GetRequiredService<LegateConfiguration>();

            var configurationValid = configuration.Validate(out var results);
            if (!configurationValid)
            {
                foreach (var result in results)
                {
                    logger.Error(result);
                }

                logger.Info("Configruations are invalid, will shutdown.");
            }

            host.Start();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseLamar()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureServices(services => { services.AddControllers(); });
                })
                .SetupLogging();
    }
}