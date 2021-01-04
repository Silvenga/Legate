using System;
using System.Reflection;
using System.Threading.Tasks;
using Lamar.Microsoft.DependencyInjection;
using Legate.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;

namespace Legate
{
    public static class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task Main(string[] args)
        {
            Logging.Configure();
            try
            {
                await RunAsync(args);
                Logger.Info("Graceful exit.");
            }
            catch (Exception e)
            {
                Logger.Error(e, "An unhandled exception occured.");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static async Task RunAsync(string[] args)
        {
            Logger.Info(@" _                          _        ");
            Logger.Info(@"| |                        | |       ");
            Logger.Info(@"| |      ___   __ _   __ _ | |_  ___ ");
            Logger.Info(@"| |     / _ \ / _` | / _` || __|/ _ \");
            Logger.Info(@"| |____|  __/| (_| || (_| || |_|  __/");
            Logger.Info(@"\_____/ \___| \__, | \__,_| \__|\___|");
            Logger.Info(@"               __/ |                 ");
            Logger.Info(@"              |___/                  ");
            Logger.Info("");
            Logger.Info($"Legate {Assembly.GetEntryAssembly()?.GetName().Version} has begun.");

            var host = CreateHostBuilder(args).Build();
            var configuration = host.Services.GetRequiredService<LegateConfiguration>();

            var configurationValid = configuration.Validate(out var results);
            if (!configurationValid)
            {
                foreach (var result in results)
                {
                    Logger.Error(result);
                }

                Logger.Info("Configruations are invalid, will shutdown.");
            }

            Logger.Info("Startup checks completed, starting API and workers.");

            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .UseLamar()
                       .ConfigureWebHostDefaults(webBuilder =>
                       {
                           webBuilder.UseStartup<Startup>();
                           webBuilder.ConfigureServices(services => { services.AddControllers(); });
                       })
                       .SetupLogging();
        }
    }
}