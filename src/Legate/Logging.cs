using NLog;
using NLog.Config;
using NLog.Targets;

namespace Legate
{
    public static class Logging
    {
        public static void Configure()
        {
            var config = new LoggingConfiguration();

            var consoleTarget = new ColoredConsoleTarget("console")
            {
                Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} ${level:uppercase=true}: ${message:WithException=true}"
            };
            config.AddTarget(consoleTarget);
            config.AddRuleForAllLevels(consoleTarget);

            LogManager.Configuration = config;
        }
    }
}